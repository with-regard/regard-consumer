using System;
using System.Diagnostics;
using System.Threading.Tasks;
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

            // TODO: To log the event, we need a session ID

            // TODO: We need to treat events that start a session specially

            // We don't translate the event
            return input;
        }
    }
}
