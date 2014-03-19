using System;
using System.Threading.Tasks;
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
    public class StoreAzureTable : IPipelineStage
    {
        private readonly IFlatTableTarget m_Target;

        public StoreAzureTable(string connectionString, string tableName)
        {
            m_Target = new AzureFlatTableTarget(connectionString, tableName);
        }

        public StoreAzureTable(IFlatTableTarget target)
        {
            if (target == null) throw new ArgumentNullException("target");
            m_Target = target;
        }

        /// <summary>
        /// Causes this pipeline stage to process an event
        /// </summary>
        public async Task<IRegardEvent> Process(IRegardEvent input)
        {
            // Generate the entity
            var entity = new FlatEventEntity
                {
                    Product         = input.Product(),
                    Organization    = input.Organization(),
                    Payload         = input.Payload()
                };

            // Use the organisation/product as the partition key
            entity.PartitionKey = StorageUtil.SanitiseKey(input.Organization() + "/" + input.Product());

            // It's not clear at this point how we'll identify rows, so we're using a GUID as the row key for the moment
            // TODO: I imagine that timestamp + serial or something similar would make sense here
            entity.RowKey = StorageUtil.SanitiseKey(Guid.NewGuid().ToString());

            // Store in the table
            await m_Target.Insert(entity);

            // Done
            return input;
        }
    }
}
