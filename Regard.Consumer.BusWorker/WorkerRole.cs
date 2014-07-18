using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using Regard.Consumer.Logic;
using Regard.Consumer.Logic.Api;
using Regard.Consumer.Logic.Data;
using Regard.Consumer.Logic.Pipeline;

namespace Regard.Consumer.BusWorker
{
    public class WorkerRole : RoleEntryPoint
    {
        const string c_QueueName = "ProcessingQueue";
        private const int c_BatchSize = 20;
        private SubscriptionClient m_Client;
        private readonly ManualResetEvent m_CompletedEvent = new ManualResetEvent(false);
        private QueryNotifier m_Notifier;

        /// <summary>
        /// The event processing pipeline
        /// </summary>
        private IPipeline m_EventPipeline;

        public override void Run()
        {
            Trace.WriteLine("Starting processing of messages");

            if (m_Notifier == null)
            {
                Trace.TraceWarning("No query notifier is available: queries may run slowly");
            }

            try
            {
                while (!m_CompletedEvent.WaitOne(0))                  // CompletedEvent gets signalled when it's time to finish
                {
                    var messages = m_Client.ReceiveBatch(c_BatchSize, TimeSpan.FromSeconds(5)).ToList();
                    if (messages.Count > 0)
                    {
                        // Process the messages
                        Task.Run(async () =>
                        {
                            var completedMessageLockTokens = new List<Guid>();
                            foreach (var receivedMessage in messages)
                            {
                                try
                                {
                                    // Message data is a JSON string
                                    Stream rawMessage = receivedMessage.GetBody<Stream>();

                                    IRegardEvent processedEvent;
                                    using (var memoryStream = new MemoryStream())
                                    {
                                        rawMessage.CopyTo(memoryStream);
                                        var body = Encoding.UTF8.GetString(memoryStream.ToArray());
                                        // Run through the pipeline
                                        processedEvent = await m_EventPipeline.Process(RegardEvent.Create(body));
                                    }

                                    // Report any errors to the trace
                                    if (processedEvent.Error() != null)
                                    {
                                        // TODO: protect against bad event spamming
                                        // Not clear how we want to do this at the moment. The endpoint should probably reject these events to stop them
                                        // occupying too many resources, so this part should probably send a message back when it wants a source to be
                                        // rejected.
                                        Trace.TraceError("Rejected event: {0}", processedEvent.Error());
                                    }

                                    // Complete the message
                                    completedMessageLockTokens.Add(receivedMessage.LockToken);
                                }
                                catch (Exception e)
                                {
                                    // Handle any message processing specific exceptions here
                                    Trace.TraceError("Exception during event processing: {0}", e.Message);

                                    // TODO: the endpoint should timeout or reject any event source that causes an exception here in order to maintain quality of service
                                }
                            }

                            // Mark these messages as completed
                            m_Client.CompleteBatch(completedMessageLockTokens);
                        }).Wait();
                    }
                }
            }
            catch (Exception e)
            {
                Trace.TraceError("Fatal error while processing messages", e);
            }
        }

        public override bool OnStart()
        {
            // We'll need some background threads to complete pending requests
            ThreadPool.SetMaxThreads(40, 100);

            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 12;

            // Create the processing pipeline
            string storageConnectionString  = CloudConfigurationManager.GetSetting("Regard.Storage.ConnectionString");
            string eventTableName           = CloudConfigurationManager.GetSetting("Regard.Storage.EventTable");
            string customerTableName        = CloudConfigurationManager.GetSetting("Regard.Storage.CustomerTable");
            string healthCheckSecret        = CloudConfigurationManager.GetSetting("Regard.HealthCheck.SharedSecret");

            var eventTable                  = new AzureFlatTableTarget(storageConnectionString, eventTableName);
            var customerTable               = new AzureFlatTableTarget(storageConnectionString, customerTableName);

            // Create the query notifier
            string queryServiceBusConnectionString  = CloudConfigurationManager.GetSetting("Regard.ServiceBus.QueryUpdate.ConnectionString");
            string queryEventTopic                  = CloudConfigurationManager.GetSetting("Regard.ServiceBus.QueryUpdate.EventTopic");

            QueryNotifier queryNotifier = null;
            if (!string.IsNullOrEmpty(queryServiceBusConnectionString) && !string.IsNullOrEmpty(queryEventTopic))
            {
                queryNotifier = new QueryNotifier(queryServiceBusConnectionString, queryEventTopic);
            }

            // For now we're just storing the data in the table
            m_EventPipeline = new DefaultDataStorePipeline(eventTable, customerTable, queryNotifier, healthCheckSecret);

            // Create the queue if it does not exist already
            string serviceBusConnectionString   = CloudConfigurationManager.GetSetting("Regard.ServiceBus.ConnectionString");
            string topic                        = CloudConfigurationManager.GetSetting("Regard.ServiceBus.EventTopic");
            string subscriptionName             = CloudConfigurationManager.GetSetting("Regard.ServiceBus.SubscriptionName");

            // Get the namespace for the queue
            var regardNamespace = NamespaceManager.CreateFromConnectionString(serviceBusConnectionString);

            // Create the topic if it doesn't already exist
            if (!regardNamespace.TopicExists(topic))
                regardNamespace.CreateTopic(topic);

            // Create the subscription
            if (!regardNamespace.SubscriptionExists(topic, subscriptionName))
                regardNamespace.CreateSubscription(topic, subscriptionName);

            SubscriptionClient subscriptionClient = SubscriptionClient.CreateFromConnectionString(serviceBusConnectionString, topic, subscriptionName);

            // Initialize the connection to Service Bus Queue
            m_Client = subscriptionClient;
            return base.OnStart();
        }

        public override void OnStop()
        {
            // Close the connection to Service Bus Queue
            m_Client.Close();
            m_CompletedEvent.Set();
            base.OnStop();
        }
    }
}
