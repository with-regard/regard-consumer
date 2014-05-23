using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Regard.Consumer.Logic.Api;
using Regard.Query;
using Regard.Query.Api;

namespace Regard.Consumer.Logic.Pipeline
{
    /// <summary>
    /// Stores data for an event using the Regard Query engine
    /// </summary>
    public class StoreQueryEngine : IPipelineStage
    {
        /// <summary>
        /// Synchronisation object, mainly used to ensure we only initialise once
        /// </summary>
        private readonly object m_Sync = new object();

        /// <summary>
        /// Task that will eventually create a data store
        /// </summary>
        private Task<IRegardDataStore> m_AwaitingDataStore;

        /// <summary>
        /// The data store that this stage will use
        /// </summary>
        private IRegardDataStore m_DataStore;

        /// <summary>
        /// True if this object has been initialised
        /// </summary>
        private bool m_Initialised;

        public StoreQueryEngine(IRegardDataStore dataStore = null)
        {
            // Remember the data store
            m_DataStore = dataStore;

            // We'll need to ask the factory for a new data store if 
            if (m_DataStore != null)
            {
                m_Initialised = true;
            }
            else
            {
                m_Initialised = false;
            }
        }

        public async Task<IRegardEvent> Process(IRegardEvent input)
        {
            // Initialise this object if it's not already initialised
            // We can't lock across an await, so we have to do this in a much more complicated way.
            lock (m_Sync)
            {
                if (!m_Initialised)
                {
                    // We'll be initialised no matter what the outcome
                    m_Initialised = true;

                    // Start waiting for a data store if none is set
                    if (m_AwaitingDataStore == null && m_DataStore == null)
                    {
                        m_AwaitingDataStore = DataStoreFactory.CreateDefaultDataStore();
                    }
                }
            }

            // Acquire the data store
            IRegardDataStore dataStore;
            Task<IRegardDataStore> awaitingDataStore;

            lock (m_Sync)
            {
                dataStore           = m_DataStore;
                awaitingDataStore   = m_AwaitingDataStore;
            }

            // Wait for the data store to 
            if (dataStore == null)
            {
                if (awaitingDataStore != null)
                {
                    // We don't have an actual data store, but one is being made. Wait for it to finish

                    // TODO: what happens if two things await this in parallel?
                    try
                    {
                        dataStore = await awaitingDataStore;
                    }
                    catch (InvalidOperationException)
                    {
                        // Occurs if the factory cannot find a data store configured
                        dataStore = null;
                        Trace.WriteLine("StoreQueryEngine: there is no data store configured");
                    }

                    lock (m_Sync)
                    {
                        m_DataStore         = dataStore;
                        m_AwaitingDataStore = null;
                    }
                }
                else
                {
                    // We don't have a data store and none is being made. Therefore, we won't be able to log this event
                    return input;
                }
            }

            if (dataStore == null)
            {
                return input;
            }

            // Events logged by this system need a particular payload format
            string payload = input.Payload();
            if (string.IsNullOrEmpty(payload))
            {
                Trace.WriteLine("StoreQueryEngine: ignoring event with no payload");
                return input;
            }

            // The payload must be formatted as JSON
            JObject jsonPayload;
            try
            {
                jsonPayload = JObject.Parse(payload);
            }
            catch (JsonException e)
            {
                Trace.WriteLine("StoreQueryEngine: ignoring event which is not JSON");
                return input;
            }

            // It must contain a session ID (stored as 'session-id'), which should be a valid GUID
            JToken sessionIdToken;

            if (!jsonPayload.TryGetValue("session-id", out sessionIdToken))
            {
                Trace.WriteLine("StoreQueryEngine: ignoring event with no session ID");
                return input;
            }

            if (sessionIdToken.Type != JTokenType.String)
            {
                Trace.WriteLine("StoreQueryEngine: ignoring event with a non-string session ID");
                return input;
            }

            Guid sessionId;
            if (!Guid.TryParse(sessionIdToken.Value<string>(), out sessionId))
            {
                Trace.WriteLine("StoreQueryEngine: ignoring event with a session ID that isn't a GUID");
            }

            // It may contain a user ID. It must contain a user ID in order to instantiate a session.
            // User id is stored in 'session-id'
            JToken  userIdToken;
            Guid    userId = Guid.Empty;

            if (jsonPayload.TryGetValue("user-id", out userIdToken))
            {
                if (userIdToken.Type == JTokenType.String)
                {
                    if (!Guid.TryParse(userIdToken.Value<string>(), out userId))
                    {
                        userId = Guid.Empty;
                    }
                }
            }

            // It may instruct us to start a new session
            // Any event containing a 'new-session' and a 'user-id' field is treated this way
            bool startNewSession = false;

            if (userId != Guid.Empty && sessionId != Guid.Empty)
            {
                // Empty user ID would create a bad session
                // Empty session ID means 'create an arbitrary session ID' which is not the behaviour we want
                startNewSession = true;
            }

            // We need to treat events that start a session specially
            if (startNewSession)
            {
                // Implies that the user ID and session ID are valid (see code above)
                // The data store should support starting the same session multiple times, so this should be OK
                // It might be an improvement to avoid restarting sessions that we know are already running, but this depends on data store implementation
                await dataStore.EventRecorder.StartSession(input.Organization(), input.Product(), userId, sessionId);
            }

            // Log this event
            // TODO: it's probably redundant to log the user info, session ID here so we could take it out of the logged payload
            await dataStore.EventRecorder.RecordEvent(sessionId, jsonPayload);

            // We don't translate the event
            return input;
        }
    }
}
