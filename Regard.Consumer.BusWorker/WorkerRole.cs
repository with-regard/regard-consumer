using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

namespace WorkerRoleWithSBQueue1
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
                        // Process the message
                        Trace.WriteLine("Processing Service Bus message: " + receivedMessage.SequenceNumber.ToString());

                        // Message data is a JSON string
                        var rawMessage = receivedMessage.GetBody<string>();

                        // Run through the pipeline
                        var processedEvent = await m_EventPipeline.Process(RegardEvent.Create(rawMessage));

                        // Report any errors to the trace
                        if (processedEvent.Error() != null)
                        {
                            // TODO: protect against bad event spamming
                            Trace.TraceError("Rejected event: {0}", processedEvent.Error());
                        }

                        // Complete the message
                        await receivedMessage.CompleteAsync();
                    }
                    catch (Exception e)
                    {
                        // Handle any message processing specific exceptions here
                        Trace.TraceError("Exception during event processing: {0}", e.Message);
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
            string storageTableName         = CloudConfigurationManager.GetSetting("Regard.Storage.EventTable");

            // For now we're just storing the data in the table
            m_EventPipeline = new AzureTablePipeline(new AzureFlatTableTarget(storageConnectionString, storageTableName));

            // Create the queue if it does not exist already
            string serviceBusConnectionString   = CloudConfigurationManager.GetSetting("Regard.ServiceBus.ConnectionString");
            string topic                        = CloudConfigurationManager.GetSetting("Regard.ServiceBus.EventTopic");
            string subscriptionName             = CloudConfigurationManager.GetSetting("Regard.ServiceBus.SubscriptionName");

            // Get the namespace for the queue
            var regardNamespace = NamespaceManager.CreateFromConnectionString(serviceBusConnectionString);

            // Create the topic if it doesn't already exist
            if (!regardNamespace.TopicExists(topic))
                regardNamespace.CreateTopic(topic);

            if (!regardNamespace.SubscriptionExists(topic, subscriptionName))
                regardNamespace.CreateSubscription(topic, subscriptionName);

            // Create the subscription
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
