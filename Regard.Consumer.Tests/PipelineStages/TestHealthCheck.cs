using System.Threading.Tasks;
using Newtonsoft.Json;
using NUnit.Framework;
using Regard.Consumer.Logic;
using Regard.Consumer.Logic.Api;
using Regard.Consumer.Logic.Pipeline;

namespace Regard.Consumer.Tests.PipelineStages
{
    class TestHealthCheck
    {
        [Test]
        public async Task HealthCheckSetRowKey()
        {
            var healthCheck = new HealthCheckRoutingStage("TestHealthCheck");

            var input = RegardEvent.Create("Raw data shouldn't matter");

            // Set the product/organisation correctly
            input = input.WithOrganization(HealthCheckRoutingStage.c_Organization).WithProduct(HealthCheckRoutingStage.c_Product);

            // Set the payload to something correct
            var payload = JsonConvert.SerializeObject(new
                                                      {
                                                          rowkey = "TestRowKey",
                                                          rowkeysignature = SignatureUtil.Signature("TestRowKey", "TestHealthCheck")
                                                      });

            input = input.WithPayload(payload);

            // Process
            var output = await healthCheck.Process(input);

            // Check
            Assert.IsNullOrEmpty(output.Error());
            Assert.AreEqual("TestRowKey", output[EventKeys.KeyRowKey]);
        }

        [Test]
        public async Task IgnoreIncorrectSignature()
        {
            var healthCheck = new HealthCheckRoutingStage("TestHealthCheck");

            var input = RegardEvent.Create("Raw data shouldn't matter");

            // Set the product/organisation correctly
            input = input.WithOrganization(HealthCheckRoutingStage.c_Organization).WithProduct(HealthCheckRoutingStage.c_Product);

            // Set the payload to something correct
            var payload = JsonConvert.SerializeObject(new
            {
                rowkey = "TestRowKey",
                rowkeysignature = "badsignature"
            });

            input = input.WithPayload(payload);

            // Process
            var output = await healthCheck.Process(input);

            // Check
            Assert.IsNullOrEmpty(output.Error());
            Assert.IsNullOrEmpty(output[EventKeys.KeyRowKey]);
        }

        [Test]
        public async Task IgnoreIncorrectOrganisation()
        {
            var healthCheck = new HealthCheckRoutingStage("TestHealthCheck");

            var input = RegardEvent.Create("Raw data shouldn't matter");

            // Set the product/organisation correctly
            input = input.WithOrganization("IncorrectOrganisation").WithProduct(HealthCheckRoutingStage.c_Product);

            // Set the payload to something correct
            var payload = JsonConvert.SerializeObject(new
            {
                rowkey = "TestRowKey",
                rowkeysignature = SignatureUtil.Signature("TestRowKey", "TestHealthCheck")
            });

            input = input.WithPayload(payload);

            // Process
            var output = await healthCheck.Process(input);

            // Check
            Assert.IsNullOrEmpty(output.Error());
            Assert.IsNullOrEmpty(output[EventKeys.KeyRowKey]);
        }
    }
}
