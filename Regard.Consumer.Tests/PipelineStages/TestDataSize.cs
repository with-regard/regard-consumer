using System.Threading.Tasks;
using NUnit.Framework;
using Regard.Consumer.Logic;
using Regard.Consumer.Logic.Api;
using Regard.Consumer.Logic.Data;
using Regard.Consumer.Logic.Pipeline;

namespace Regard.Consumer.Tests.PipelineStages
{
    /// <summary>
    /// Tests the data sizing stage
    /// </summary>
    [TestFixture]
    public class TestDataSize
    {
        [Test]
        public async Task PassGoodData()
        {
            var input = RegardEvent.Create("Raw data shouldn't matter at this point");

            // Create a populated event
            var populated = input.WithOrganization("Red Gate Software").WithProduct("Regard Tests").WithPayload("{}");

            // Try it out on the table storage stage
            var stage = new CheckDataSize();
            var result = await stage.Process(populated);

            Assert.IsNullOrEmpty(result.Error());
        }

        [Test]
        public async Task ErrorIfOrganizationTooLong()
        {
            var input = RegardEvent.Create("Raw data shouldn't matter at this point");

            // Create a populated event
            var populated = input.WithOrganization(new string('x', CheckDataSize.c_MaxIdentifier + 1)).WithProduct("Regard Tests").WithPayload("{}");

            // Try it out on the table storage stage
            var stage = new CheckDataSize();
            var result = await stage.Process(populated);

            Assert.IsNotNullOrEmpty(result.Error());
        }

        [Test]
        public async Task ErrorIfProductTooLong()
        {
            var input = RegardEvent.Create("Raw data shouldn't matter at this point");

            // Create a populated event
            var populated = input.WithOrganization("Red Gate Software").WithProduct(new string('x', CheckDataSize.c_MaxIdentifier+1)).WithPayload("{}");

            // Try it out on the table storage stage
            var stage = new CheckDataSize();
            var result = await stage.Process(populated);

            Assert.IsNotNullOrEmpty(result.Error());
        }

        [Test]
        public async Task ErrorIfPayloadTooLong()
        {
            var input = RegardEvent.Create("Raw data shouldn't matter at this point");

            // Create a populated event
            var populated = input.WithOrganization("Red Gate Software").WithProduct("Regard Tests").WithPayload("\"" + new string('x', CheckDataSize.c_MaxCharacters-1) /* Including the quotes = exactly 1 character too long */ + "\"");

            // Try it out on the table storage stage
            var stage = new CheckDataSize();
            var result = await stage.Process(populated);

            Assert.IsNotNullOrEmpty(result.Error());
        }
    }
}
