using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json.Linq;

namespace Regard.Consumer.SelfTest.QueryAPI
{
    public class QueryReturnsDataOnceThereAreMatchingEvents : ITest
    {
        public async Task<JObject> Run()
        {
            const int maxAttempts = 6;

            var testEvent = new JObject();
            testEvent["event-type"] = "test";

            // Send a couple of events
            await Task.WhenAll(new[]
            {
                QueryUtil.SendEvent((JObject) testEvent.DeepClone()),
                QueryUtil.SendEvent((JObject) testEvent.DeepClone())
            });

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                // Wait 5 seconds between attempts
                await Task.Delay(TimeSpan.FromSeconds(5));

                Trace.WriteLine("Attempting to verify query results, attempt " + attempt);

                // Run the query
                var queryUrl = "product/v1/" + QueryData.OrganizationName + "/" + QueryData.ThisSessionProductName + "/run-query/test";
                var response = await QueryUtil.RunQuery(queryUrl, null, "GET");
                var resultObj = response.Item1;

                var count = resultObj["Results"][0]["EventCount"].Value<int>();
                Trace.WriteLine("Event count is " + count);

                if (count == 2)
                {
                    // Success
                    return JObject.FromObject(new {Result = "Found two events"});
                }
                else if (count > 2)
                {
                    // Failure
                    return JObject.FromObject(new { Error = "Query returned too many events" });
                }
            }

            // Failure
            return JObject.FromObject(new { Error = "Query never received the events" });
        }

        public string Name { get { return "QueryReturnsDataOnceThereAreMatchingEvents"; } }
    }
}