using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Regard.Consumer.Logic.Api;
using Regard.Consumer.Logic.Data;

namespace Regard.Consumer.Logic.Pipeline
{
    /// <summary>
    /// Stores data for an event in an Azure table, using a very simple scheme
    /// </summary>
    /// <remarks>
    /// This is mainly intended to demonstrate that we can store th
    /// </remarks>
    class StoreAzureTable : IPipelineStage
    {
        /// <summary>
        /// The storage account that this will use
        /// </summary>
        private readonly CloudStorageAccount m_StorageAccount;

        /// <summary>
        /// The table client that we'll store data in
        /// </summary>
        private readonly CloudTableClient m_TableClient;

        /// <summary>
        /// The target table for this object
        /// </summary>
        private readonly CloudTable m_Table;

        public StoreAzureTable(string connectionString, string tableName)
        {
            // Setup
            m_StorageAccount    = CloudStorageAccount.Parse(connectionString);
            m_TableClient       = m_StorageAccount.CreateCloudTableClient();
            m_Table             = m_TableClient.GetTableReference(tableName);
            m_Table.CreateIfNotExists();
        }

        /// <summary>
        /// Causes this pipeline stage to process an event
        /// </summary>
        public async Task<IRegardEvent> Process(IRegardEvent input)
        {
            // Generate the entity
            var entity = new FlatEventEntity
                {
                    Product         = input.Product,
                    Organization    = input.Organization,
                    Payload         = input.Payload
                };

            // Store in the table
            var insertNewEvent = TableOperation.Insert(entity);
            await m_Table.ExecuteAsync(insertNewEvent);

            // Done
            return input;
        }
    }
}
