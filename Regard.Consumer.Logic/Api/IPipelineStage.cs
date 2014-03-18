using System.Threading.Tasks;

namespace Regard.Consumer.Logic.Api
{
    /// <summary>
    /// Represents a stage in an event processing pipeline
    /// </summary>
    public interface IPipelineStage
    {
        /// <summary>
        /// Causes this pipeline stage to process an event
        /// </summary>
        Task<IRegardEvent> Process(IRegardEvent input);
    }
}
