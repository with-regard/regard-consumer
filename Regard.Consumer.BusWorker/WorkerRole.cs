using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
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
        // The name of your queue
        const string QueueName = "ProcessingQueue";

        SubscriptionClient Client;
        ManualResetEvent CompletedEvent = new ManualResetEvent(false);

        /// <summary>
        /// The event processing pipeline
        /// </summary>
        private IPipeline m_EventPipeline;

        public override void Run()
        {
            Trace.WriteLine("Starting processing of messages");

            // Initiates the message pump and callback is invoked for each message that is received, calling close on the client will stop the pump.
            Client.OnMessageAsync(async (receivedMessage) =>
                {
                    try
                    {
                        // Message data is a JSON string
                        var rawMessage = receivedMessage.GetBody<string>();

                        // Run through the pipeline
                        var processedEvent = await m_EventPipeline.Process(RegardEvent.Create(rawMessage));

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
                        await receivedMessage.CompleteAsync();
                    }
                    catch (Exception e)
                    {
                        // Handle any message processing specific exceptions here
                        Trace.TraceError("Exception during event processing: {0}", e.Message);

                        // TODO: the endpoint should timeout or reject any event source that causes an exception here in order to maintain quality of service
                    }
                });

            CompletedEvent.WaitOne();
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 12;

            // Create the processing pipeline
            string storageConnectionString  = CloudConfigurationManager.GetSetting("Regard.Storage.ConnectionString");
            string eventTableName           = CloudConfigurationManager.GetSetting("Regard.Storage.EventTable");
            string customerTableName        = CloudConfigurationManager.GetSetting("Regard.Storage.CustomerTable");
            string healthCheckSecret        = CloudConfigurationManager.GetSetting("Regard.HealthCheck.SharedSecret");

            var eventTable                  = new AzureFlatTableTarget(storageConnectionString, eventTableName);
            var customerTable               = new AzureFlatTableTarget(storageConnectionString, customerTableName);

            // For now we're just storing the data in the table
            m_EventPipeline = new DefaultDataStorePipeline(eventTable, customerTable, healthCheckSecret);

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
            Client = subscriptionClient;
            return base.OnStart();
        }

        public override void OnStop()
        {
            // Close the connection to Service Bus Queue
            Client.Close();
            CompletedEvent.Set();
            base.OnStop();
        }
    }
}
