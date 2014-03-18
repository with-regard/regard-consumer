using System.Collections;
using System.Collections.Generic;

namespace Regard.Consumer.Logic.Api
{
    /// <summary>
    /// Represents a series of operations on an event
    /// </summary>
    public interface IPipeline
    {
        /// <summary>
        /// The stages in this pipeline
        /// </summary>
        IEnumerable<IPipelineStage> Stages { get; }
    }
}
