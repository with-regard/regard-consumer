using System.Threading.Tasks;
using NUnit.Framework;
using Regard.Consumer.Logic;
using Regard.Consumer.Logic.Api;
using Regard.Consumer.Logic.Pipeline;

namespace Regard.Consumer.Tests.MissingFeatures.PipelineStages
{
    class TestCheckOrganization
    {
        [Test]
        [ExpectedException(typeof(AssertionException))]                             // The consumer functions correctly without this feature; it is intended as a defence against attacks that dump data into the service
        public async Task RejectUnknownOrganization()
        {
            // Organizations that don't exist shouldn't get events added to the table
            var stage = new CheckOrganization(new TestTableTarget());
            var input = RegardEvent.Create("Raw data shouldn't matter").WithOrganization("DoesntExist");

            var result = await stage.Process(input);

            Assert.IsNotNullOrEmpty(result.Error());
        }
    }
}
