using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json.Linq;
using Regard.Consumer.SelfTest.QueryAPI;

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
            return Request.CreateResponse(HttpStatusCode.OK, SelfTest.TestResults.Results);
        }
    }
}
