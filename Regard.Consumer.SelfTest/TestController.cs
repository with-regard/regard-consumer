using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json;

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
                return Request.CreateResponse(HttpStatusCode.OK, JsonConvert.SerializeObject(results));
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.OK, JsonConvert.SerializeObject(new
                                                                                             {
                                                                                                 Status = "Tests not finished yet"
                                                                                             }));
            }
        }
    }
}
