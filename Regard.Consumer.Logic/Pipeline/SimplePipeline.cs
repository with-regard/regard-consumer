using System.Collections.Generic;
using Regard.Consumer.Logic.Api;

namespace Regard.Consumer.Logic.Pipeline
{
    /// <summary>
    /// Basic implementation of the pipeline
    /// </summary>
    public class SimplePipeline : IPipeline
    {
        public SimplePipeline(IEnumerable<IPipelineStage> stages)
        {
            if (stages == null) stages = new IPipelineStage[0];

            Stages = new List<IPipelineStage>(stages);
        }

        /// <summary>
        /// The stages in this pipeline
        /// </summary>
        public IEnumerable<IPipelineStage> Stages
        {
            get; private set;
        }
    }
}
