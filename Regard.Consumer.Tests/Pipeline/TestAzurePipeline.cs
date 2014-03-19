using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;
using NUnit.Framework;
using Regard.Consumer.Logic;
using Regard.Consumer.Logic.Api;
using Regard.Consumer.Logic.Data;
using Regard.Consumer.Logic.Pipeline;

namespace Regard.Consumer.Tests.Pipeline
{
    [TestFixture]
    public class TestAzurePipeline
    {
        /// <summary>
        /// A very simple example of an event
        /// </summary>
        public const string c_TestRawData = "{\"schema_version\":256,\"organization\":\"Red Gate Software\",\"product\":\"Regard Tests\",\"payload\":{\"data\":\"something\"}}";

        [Test]
        public async Task StoreOneEvent()
        {
            // More of an integration test than a unit test
            var tableTarget = new TestTableTarget();
            var azurePipeline = new AzureTablePipeline(tableTarget);

            var input = RegardEvent.Create(c_TestRawData);
            var result = await azurePipeline.Process(input);

            // Should be no errors
            Assert.IsNullOrEmpty(result.Error());

            // Should have inserted one entity
            Assert.AreEqual(1, tableTarget.InsertedEntities.Count);

            // Should be a flat event entity
            var entity = tableTarget.InsertedEntities[0] as FlatEventEntity;
            Assert.IsNotNull(entity);

            // Should have the values in the test data
            Assert.AreEqual("Red Gate Software", entity.Organization);
            Assert.AreEqual("Regard Tests", entity.Product);
            Assert.IsNotNull(entity.PartitionKey);
            Assert.IsNotNull(entity.RowKey);
            Assert.IsNotNullOrEmpty(entity.Payload);
        }
    }
}
