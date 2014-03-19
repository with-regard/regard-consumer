using System.Threading.Tasks;
using NUnit.Framework;
using Regard.Consumer.Logic;
using Regard.Consumer.Logic.Api;
using Regard.Consumer.Logic.Pipeline;

namespace Regard.Consumer.Tests.Pipeline
{
    [TestFixture]
    public class TestPipeline
    {
        /// <summary>
        /// Stage that sets an arbitrary event key to 'OK'
        /// </summary>
        class TestStage : IPipelineStage
        {
            private string m_StageKey;

            public TestStage(string stageKey)
            {
                m_StageKey = stageKey;
            }

            /// <summary>
            /// Causes this pipeline stage to process an event
            /// </summary>
            public async Task<IRegardEvent> Process(IRegardEvent input)
            {
                return input.With(m_StageKey, "OK");
            }
        }

        [Test]
        public async Task ProcessOneStage()
        {
            var stage = new TestStage("Stage1");
            var pipeline = new SimplePipeline(new[] {stage});

            var result = await pipeline.Process(RegardEvent.Create(""));

            Assert.AreEqual("OK", result["Stage1"]);
        }

        [Test]
        public async Task ProcessThreeStages()
        {
            var stage1 = new TestStage("Stage1");
            var stage2 = new TestStage("Stage2");
            var stage3 = new TestStage("Stage3");
            var pipeline = new SimplePipeline(new[] { stage1, stage2, stage3 });

            var result = await pipeline.Process(RegardEvent.Create(""));

            Assert.AreEqual("OK", result["Stage1"]);
            Assert.AreEqual("OK", result["Stage2"]);
            Assert.AreEqual("OK", result["Stage3"]);
        }


        [Test]
        public async Task StopOnError()
        {
            var stage1 = new TestStage("Stage1");
            var stage2 = new TestStage(EventKeys.KeyError);
            var stage3 = new TestStage("Stage3");
            var pipeline = new SimplePipeline(new[] { stage1, stage2, stage3 });

            var result = await pipeline.Process(RegardEvent.Create(""));

            Assert.AreEqual("OK", result["Stage1"]);
            Assert.AreEqual("OK", result.Error());
            Assert.IsNull(result["Stage3"]);
        }
    }
}
