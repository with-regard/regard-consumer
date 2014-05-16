using System.Threading.Tasks;
using NUnit.Framework;
using Regard.Consumer.Logic;
using Regard.Consumer.Logic.Api;
using Regard.Consumer.Logic.Pipeline;

namespace Regard.Consumer.Tests.PipelineStages
{
    [TestFixture]
    public class TestCheckOrganization
    {
        [Test]
        public async Task AlwaysAcceptWithRegard()
        {
            // The 'WithRegard' organisation should be accepted always (it's used for the health checks, so these will fail if it isn't)
            var stage = new CheckOrganization(new TestTableTarget());
            var input = RegardEvent.Create("Raw data shouldn't matter").WithOrganization("WithRegard");

            var result = await stage.Process(input);

            Assert.IsNullOrEmpty(result.Error());
        }
    }
}
 