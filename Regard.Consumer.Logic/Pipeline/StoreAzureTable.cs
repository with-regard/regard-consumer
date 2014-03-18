using System;
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

            // Use the organisation/product as the partition key
            // TODO: '/' is an invalid character in the partition key. I can't find where azure documents what goes here; the docs for TableEntity helpfully state that this contains the partition key and nothing else
            entity.PartitionKey = input.Organization + "/" + input.Product;

            // It's not clear at this point how we'll identify rows, so we're using a GUID as the row key for the moment
            // TODO: I imagine that timestamp + serial or something similar would make sense here
            // TODO: partition keys can only contain certain characters, it seems likely that row keys have the same limitations
            entity.RowKey = Guid.NewGuid().ToString();

            // Store in the table
            var insertNewEvent = TableOperation.Insert(entity);
            await m_Table.ExecuteAsync(insertNewEvent);

            // Done
            return input;
        }
    }
}
