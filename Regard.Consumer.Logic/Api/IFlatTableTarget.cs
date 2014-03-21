using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace Regard.Consumer.Logic.Api
{
    /// <summary>
    /// Interface implemented by objects that work like an Azure flat table as a storage target
    /// </summary>
    /// <remarks>
    /// Abstraction is used to improve testability (specifically, we need to know that all pipeline stages are executed)
    /// </remarks>
    public interface IFlatTableTarget
    {
        /// <summary>
        /// Inserts an entity into this table
        /// </summary>
        Task Insert(ITableEntity entity);

        /// <summary>
        /// Returns null if no entity exists, otherwise the entity with a particular partition and row key
        /// </summary>
        Task<ITableEntity> Find(string partitionKey, string rowKey);
    }
}
