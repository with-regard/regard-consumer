using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json.Linq;

namespace Regard.Consumer.BusWorker
{
    /// <summary>
    /// Class whose job it is to notify the query nodes about any product/organizations that are receiving a lot of events
    /// </summary>
    /// <remarks>
    /// In order to make sure that query results are returned quickly, we need to ensure that the actualised results are
    /// regularly updated when there are many incoming events. However, updating on every event is inefficient, so we
    /// wait for enough events to arrive.
    /// <para/>
    /// When scaling, this works best if nodes choose a set of products/organizations to 'own', as if events for one
    /// product end up distributed across multiple nodes then this will make queries update less frequently. It seems
    /// highly unlikely that any one product will ever generate enough events to require multiple nodes to handle the
    /// ingest.
    /// </remarks>
    public class QueryNotifier
    {
        private readonly object m_Sync = new object();

        /// <summary>
        /// Number of events to receive against a product before we ask the query node to update its results
        /// </summary>
        private const int c_NotificationCount = 1000;

        /// <summary>
        /// Organization mapped to product mapped to event counts
        /// </summary>
        private readonly Dictionary<string, Dictionary<string, int>> m_Counts = new Dictionary<string, Dictionary<string, int>>();

        /// <summary>
        /// Service bus topic where notifications will be posted
        /// </summary>
        private readonly TopicClient m_NotificationTopic;

        public QueryNotifier(string serviceBusConnectionString, string serviceBusTopic)
        {
            var serviceBusNamespace = NamespaceManager.CreateFromConnectionString(serviceBusConnectionString);

            if (!serviceBusNamespace.TopicExists(serviceBusTopic))
                serviceBusNamespace.CreateTopic(serviceBusTopic);

            m_NotificationTopic = TopicClient.CreateFromConnectionString(serviceBusConnectionString, serviceBusTopic);
        }

        /// <summary>
        /// Counts one or more events against a particular product
        /// </summary>
        public void CountEvent(string organization, string product, int numEvents = 1)
        {
            lock (m_Sync)
            {
                Dictionary<string, int> productCounts;

                if (!m_Counts.TryGetValue(organization, out productCounts))
                {
                    productCounts = m_Counts[organization] = new Dictionary<string, int>();
                }

                int lastCount;
                if (!productCounts.TryGetValue(product, out lastCount))
                {
                    lastCount = productCounts[product] = 0;
                }

                if (lastCount > c_NotificationCount)
                {
                    // Start notifying the target about the update
                    Task.Run(async () => await NotifyNeedQueryUpdate(organization, product));

                    // Count goes back to 0
                    lastCount = 0;
                }

                productCounts[product] = lastCount + numEvents;
            }
        }

        /// <summary>
        /// Sends a suitable notification to the stream
        /// </summary>
        private async Task NotifyNeedQueryUpdate(string organization, string product)
        {
            // Create the message for this topic
            JObject message = new JObject();

            message["Organization"] = organization;
            message["Product"]      = product;

            // Encode as UTF-8
            var stringMessage = message.ToString();
            byte[] buffer = Encoding.UTF8.GetBytes(stringMessage);

            // Send
            using (var encodedStream = new MemoryStream(buffer, 0, buffer.Length))
            {
                await m_NotificationTopic.SendAsync(new BrokeredMessage(encodedStream));
            }
        }
    }
}
