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
        private Task m_RunningTests;
        private readonly ITest m_Tests;
        private JObject m_TestResult;

        public TestController()
        {
            // Create the tests
            m_Tests = new QueryApiTests();
            m_TestResult = JObject.FromObject(new
            {
                Status = "Tests not yet completed"
            });

            // Run the tests (in the background)
            m_RunningTests = RunTests();
        }

        /// <summary>
        /// Runs the tests for this controller
        /// </summary>
        private async Task RunTests()
        {
            var result = await m_Tests.Run();

            m_TestResult = result;
        }

        [HttpGet, Route("test-results")]
        public async Task<HttpResponseMessage> TestResults()
        {
            return Request.CreateResponse(HttpStatusCode.OK, m_TestResult);
        }
    }
}