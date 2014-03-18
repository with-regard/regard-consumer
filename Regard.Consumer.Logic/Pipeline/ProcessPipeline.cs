using System.Threading.Tasks;
using Regard.Consumer.Logic.Api;

namespace Regard.Consumer.Logic.Pipeline
{
    /// <summary>
    /// Utility methods for processing a pipeline
    /// </summary>
    public static class ProcessPipeline
    {
        /// <summary>
        /// Runs an event through a pipeline
        /// </summary>
        public static async Task<IRegardEvent> Process(this IPipeline pipeline, IRegardEvent input)
        {
            // Sanity check
            if (pipeline == null)
            {
                return RegardEvent.Create(null).WithError("No pipeline");
            }
            if (input == null)
            {
                return RegardEvent.Create(null).WithError("No input event");
            }

            // Process the stages in the pipeline
            var currentEvent = input;
            foreach (var stage in pipeline.Stages)
            {
                currentEvent = await stage.Process(currentEvent);

                if (currentEvent == null)
                {
                    // Oops, broken stage
                    return RegardEvent.Create(null).WithError("Pipeline stage returned null");
                }
                else if (!string.IsNullOrEmpty(currentEvent.Error()))
                {
                    // Stop if there's an error
                    return currentEvent;
                }
            }

            return currentEvent;
        }
    }
}
