using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Regard.Query.Serializable;

namespace Regard.Consumer.SelfTest.QueryAPI
{
    public class QueryInitiallyReturnsNoResults : ITest
    {
        public async Task<JObject> Run()
        {
            // Register the query
            var queryUrl = "product/v1/" + QueryData.OrganizationName + "/" + QueryData.ThisSessionProductName + "/run-query/test";

            var response = await QueryUtil.RunQuery(queryUrl, null, "GET");

            if (response.Item2 != HttpStatusCode.OK)
            {
                return JObject.FromObject(new
                {
                    Error = "Bad response status code",
                    StatusCode = (int)response.Item2
                });
            }

            Trace.WriteLine("QueryInitiallyReturnsNoResults: retrieved response: " + response.Item1);

            var resultObj = response.Item1;

            // Should contain Results.EventCount = 0
            if (resultObj["Results"]["EventCount"].Value<int>() != 0)
            {
                return JObject.FromObject(new {Error = "Should be no events"});
            }

            return JObject.FromObject(new  { Result = "Initial query has no results" });
        }

        public string Name { get { return "QueryInitiallyReturnsNoResults"; } }
    }
}