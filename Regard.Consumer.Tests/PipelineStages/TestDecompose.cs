using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Regard.Consumer.Logic;
using Regard.Consumer.Logic.Api;
using Regard.Consumer.Logic.Pipeline;

namespace Regard.Consumer.Tests.PipelineStages
{
    [TestFixture]
    public class TestDecompose
    {
        public const string c_NotValidPayload = "{\"schema_version\":256,\"organization\":\"Red Gate Software\",\"product\":\"Regard Tests\",\"payload\":{\"data\":\"something\"}}";
        public const string c_NoVersion = "{\"organization\":\"Red Gate Software\",\"product\":\"Regard Tests\",\"payload\":{\"data\":\"something\"}}";
        public const string c_NoOrganisation = "{\"schema_version\":256,\"product\":\"Regard Tests\",\"payload\":{\"data\":\"something\"}}";
        public const string c_NoProduct = "{\"schema_version\":256,\"organization\":\"Red Gate Software\",\"payload\":{\"data\":\"something\"}}";
        public const string c_NotJSON = "This is not JSON";

        [Test]
        public async Task DecomposeCompleteEvent()
        {
            var decompose = new DecomposeStage();
            var input = RegardEvent.Create(JsonConvert.SerializeObject(new
                                                                       {
                                                                           schema_version = 0x100,
                                                                           organization = "Red Gate Software",
                                                                           product = "Regard Tests",
                                                                           payload = new
                                                                                     {
                                                                                         prop1 = "value",
                                                                                         data = new
                                                                                                {
                                                                                                    childProp = "child value"
                                                                                                }
                                                                                     }
                                                                       }));

            var result = await decompose.Process(input);

            // Check that we got the right results
            Assert.IsNullOrEmpty(result.Error());

            Assert.AreEqual("Red Gate Software", result.Organization());
            Assert.AreEqual("Regard Tests", result.Product());
            Assert.AreEqual("256", result.Version());
            Assert.IsNotNullOrEmpty(result.Payload());
            Assert.AreEqual("child value", JsonConvert.DeserializeAnonymousType(result.Payload(), new
                                                                                                  {
                                                                                                      prop1 = "",
                                                                                                      childProp = ""
                                                                                                  }).childProp);
        }
        
        [Test]
        public async Task ErrorNotValidPayload()
        {
            var decompose = new DecomposeStage();
            var input = RegardEvent.Create(c_NotValidPayload);

            var result = await decompose.Process(input);

            Assert.IsNotNullOrEmpty(result.Error());
        }

        [Test]
        public async Task ErrorNoVersion()
        {
            var decompose = new DecomposeStage();
            var input = RegardEvent.Create(c_NoVersion);

            var result = await decompose.Process(input);

            Assert.IsNotNullOrEmpty(result.Error());
        }

        [Test]
        public async Task ErrorNoOrganisation()
        {
            var decompose = new DecomposeStage();
            var input = RegardEvent.Create(c_NoOrganisation);

            var result = await decompose.Process(input);

            Assert.IsNotNullOrEmpty(result.Error());
        }

        [Test]
        public async Task ErrorNoProduct()
        {
            var decompose = new DecomposeStage();
            var input = RegardEvent.Create(c_NoProduct);

            var result = await decompose.Process(input);

            Assert.IsNotNullOrEmpty(result.Error());
        }

        [Test]
        public async Task ErrorNotJSON()
        {
            var decompose = new DecomposeStage();
            var input = RegardEvent.Create(c_NotJSON);

            var result = await decompose.Process(input);

            Assert.IsNotNullOrEmpty(result.Error());
        }
    }
}
