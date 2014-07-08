using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json.Linq;

namespace Regard.Consumer.SelfTest
{
    /// <summary>
    /// API controller that returns the results of tests
    /// </summary>
    public class TestController : ApiController
    {
        [HttpGet, Route("test-results")]
        public async Task<HttpResponseMessage> TestResults()
        {
            var results = SelfTest.TestResults.Results;

            if (results.Any())
            {
                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(new JArray(results.ToArray())));
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.OK, JObject.FromObject(new
                                                                                    {
                                                                                        Status = "Tests not finished yet"
                                                                                    }));
            }
        }
    }
}
