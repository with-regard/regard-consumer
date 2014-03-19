using System.Threading.Tasks;
using Regard.Consumer.Logic.Api;

namespace Regard.Consumer.Logic.Pipeline
{
    /// <summary>
    /// Checks that the product in an event is valid
    /// </summary>
    public class CheckProduct : IPipelineStage
    {
        /// <summary>
        /// Causes this pipeline stage to process an event
        /// </summary>
        public async Task<IRegardEvent> Process(IRegardEvent input)
        {
            // TODO: actually check
            return input;
        }
    }
}
