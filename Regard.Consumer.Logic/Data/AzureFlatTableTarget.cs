using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Regard.Consumer.Logic.Api;

namespace Regard.Consumer.Logic.Data
{
    /// <summary>
    /// Table target that addresses an Azure flat table
    /// </summary>
    public class AzureFlatTableTarget : IFlatTableTarget
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

        public AzureFlatTableTarget(string connectionString, string tableName)
        {
            // Setup
            m_StorageAccount    = CloudStorageAccount.Parse(connectionString);
            m_TableClient       = m_StorageAccount.CreateCloudTableClient();
            m_Table             = m_TableClient.GetTableReference(tableName);
            m_Table.CreateIfNotExists();
        }

        /// <summary>
        /// Inserts an entity into this table
        /// </summary>
        public async Task Insert(ITableEntity entity)
        {
            var insertOperation = TableOperation.Insert(entity);
            await m_Table.ExecuteAsync(insertOperation);
        }

        /// <summary>
        /// Returns null if no entity exists, otherwise the entity with a particular partition and row key
        /// </summary>
        public async Task<ITableEntity> Find(string partitionKey, string rowKey)
        {
            // Create a new query for this item
            var query = new TableQuery<TableEntity>().Where(
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey),
                    TableOperators.And,
                    TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, rowKey)));

            // The entity that we found
            ITableEntity foundEntity = null;

            // Wait for the result
            var querySegment = await m_Table.ExecuteQuerySegmentedAsync(query, null);

            while (querySegment != null)
            {
                // The first entity we find becomes the result of this expression
                if (foundEntity == null && querySegment.Results != null && querySegment.Results.Count > 0)
                {
                    foundEntity = querySegment.Results[0];
                }

                // Finish off the query
                if (querySegment.ContinuationToken != null)
                {
                    querySegment = await m_Table.ExecuteQuerySegmentedAsync(query, querySegment.ContinuationToken);
                }
                else
                {
                    querySegment = null;
                }
            }

            // Return the entity that we found
            return foundEntity;
        }
    }
}
