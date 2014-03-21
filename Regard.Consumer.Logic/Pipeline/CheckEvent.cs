using System.Threading.Tasks;
using Regard.Consumer.Logic.Api;

namespace Regard.Consumer.Logic.Pipeline
{
    /// <summary>
    /// Pipeline stage that verifies an event
    /// </summary>
    public class CheckEvent : IPipelineStage
    {
        /// <summary>
        /// Causes this pipeline stage to process an event
        /// </summary>
        public async Task<IRegardEvent> Process(IRegardEvent input)
        {
            // This is currently a no-op. Here's some pseudocode for what it 'should' do eventually:

            // Check that the event is part of an active experiment or something that is being monitored

            // Check that the event doesn't contain anything that looks personally identifiable
            return input;
        }
    }
}
