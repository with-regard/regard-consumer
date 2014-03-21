using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;
using Regard.Consumer.Logic.Api;

namespace Regard.Consumer.Tests
{
    /// <summary>
    /// Table target that just stores a .NET list of entities that have been added
    /// </summary>
    class TestTableTarget : IFlatTableTarget
    {
        public TestTableTarget()
        {
            InsertedEntities = new List<ITableEntity>();
        }

        public List<ITableEntity> InsertedEntities
        {
            get;
            private set;
        }

        /// <summary>
        /// Returns null if no entity exists, otherwise the entity with a particular partition and row key
        /// </summary>
        public async Task<ITableEntity> Find(string partitionKey, string rowKey)
        {
            foreach (var entity in InsertedEntities)
            {
                if (entity.PartitionKey == partitionKey && entity.RowKey == rowKey)
                {
                    return entity;
                }
            }

            return null;
        }

        /// <summary>
        /// Inserts an entity into this table
        /// </summary>
        public async Task Insert(ITableEntity entity)
        {
            InsertedEntities.Add(entity);
        }
    }

}
