using System.Threading.Tasks;
using NUnit.Framework;
using Regard.Consumer.Logic;
using Regard.Consumer.Logic.Api;
using Regard.Consumer.Logic.Data;
using Regard.Consumer.Logic.Pipeline;

namespace Regard.Consumer.Tests.PipelineStages
{
    [TestFixture]
    public class TestStorage
    {
        [Test]
        public async Task StoreOneEvent()
        {
            var testTable = new TestTableTarget();
            var input = RegardEvent.Create("Raw data shouldn't matter at this point");

            // Create a populated event
            var populated = input.WithOrganization("Red Gate Software").WithProduct("Regard Tests").WithPayload("{}");

            // Try it out on the table storage stage
            var stage = new StoreAzureTable(testTable);
            var result = await stage.Process(populated);

            Assert.IsNullOrEmpty(result.Error());

            Assert.AreEqual(1, testTable.InsertedEntities.Count);

            var entity = testTable.InsertedEntities[0] as FlatEventEntity;

            Assert.IsNotNull(entity);
            Assert.IsNotNull(entity.PartitionKey);
            Assert.IsNotNull(entity.RowKey);
            Assert.AreEqual("Red Gate Software", entity.Organization);
            Assert.AreEqual("Regard Tests", entity.Product);
            Assert.IsNotNullOrEmpty(entity.Payload);
        }

        [Test]
        public async Task ErrorIfOrganizationTooLong()
        {
            var testTable = new TestTableTarget();
            var input = RegardEvent.Create("Raw data shouldn't matter at this point");

            // Create a populated event
            var populated = input.WithOrganization(new string('x', 1024)).WithProduct("Regard Tests").WithPayload("{}");

            // Try it out on the table storage stage
            var stage = new StoreAzureTable(testTable);
            var result = await stage.Process(populated);

            Assert.IsNotNullOrEmpty(result.Error());
            Assert.AreEqual(0, testTable.InsertedEntities.Count);
        }


        [Test]
        public async Task ErrorIfProductTooLong()
        {
            var testTable = new TestTableTarget();
            var input = RegardEvent.Create("Raw data shouldn't matter at this point");

            // Create a populated event
            var populated = input.WithOrganization("Red Gate Software Ltd").WithProduct(new string('x', 1024)).WithPayload("{}");

            // Try it out on the table storage stage
            var stage = new StoreAzureTable(testTable);
            var result = await stage.Process(populated);

            Assert.IsNotNullOrEmpty(result.Error());
            Assert.AreEqual(0, testTable.InsertedEntities.Count);
        }
    }
}
