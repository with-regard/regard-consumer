using System.Threading.Tasks;
using Regard.Consumer.Logic.Api;

namespace Regard.Consumer.Logic.Pipeline
{
    /// <summary>
    /// Verifies that the end user that an event is generated for exists and has not opted out
    /// </summary>
    class CheckUser : IPipelineStage
    {
        /// <summary>
        /// Causes this pipeline stage to process an event
        /// </summary>
        public async Task<IRegardEvent> Process(IRegardEvent input)
        {
            // TODO: actually check the user data
            return input;
        }
    }
}
