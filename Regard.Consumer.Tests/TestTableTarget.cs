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
        /// Inserts an entity into this table
        /// </summary>
        public async Task Insert(ITableEntity entity)
        {
            InsertedEntities.Add(entity);
        }
    }

}
