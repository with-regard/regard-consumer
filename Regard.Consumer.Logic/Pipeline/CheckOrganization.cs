using System.Threading.Tasks;
using Regard.Consumer.Logic.Api;

namespace Regard.Consumer.Logic.Pipeline
{
    /// <summary>
    /// Checks that the organization in an event is valid
    /// </summary>
    class CheckOrganization : IPipelineStage
    {
        /// <summary>
        /// Causes this pipeline stage to process an event
        /// </summary>
        public async Task<IRegardEvent> Process(IRegardEvent input)
        {
            // TODO: check that the organization in the event is valid, drop with an error if it is not
            return input;
        }
    }
}
