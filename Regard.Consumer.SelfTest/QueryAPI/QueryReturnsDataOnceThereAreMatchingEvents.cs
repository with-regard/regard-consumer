using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Regard.Consumer.SelfTest.QueryAPI
{
    public class QueryReturnsDataOnceThereAreMatchingEvents : ITest
    {
        public async Task<JObject> Run()  
        {
            const int maxAttempts = 6;

            var testEvent = new JObject();
            testEvent["event-data"] = "test";

            // Send a couple of events
            var sendEventResponse = await QueryUtil.SendEvent("test", (JObject) testEvent.DeepClone());
            if (sendEventResponse == HttpStatusCode.OK)
            {
                sendEventResponse = await QueryUtil.SendEvent("test", (JObject) testEvent.DeepClone());
            }

            if (sendEventResponse != HttpStatusCode.OK)
            {
                return
                    JObject.FromObject(
                        new {Error = "Endpoint did not respond correctly to events", StatusCode = sendEventResponse});
            }

            DateTime initialTime = DateTime.Now;
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                // Wait 10 seconds between attempts
                await Task.Delay(TimeSpan.FromSeconds(10));

                // Log the time between tests. Events don't need to go through the system super fast, so it may take a few seconds for the query to update
                Trace.WriteLine((DateTime.Now - initialTime).TotalSeconds + ": Attempting to verify query results, attempt " + attempt);

                // Run the query
                var queryUrl = "product/v1/" + QueryData.OrganizationName + "/" + QueryData.ThisSessionProductName + "/run-query/test";
                var response = await QueryUtil.RunQuery(queryUrl, null, "GET");
                var resultObj = response.Item1;

                var count = resultObj["Results"][0]["EventCount"].Value<int>();
                Trace.WriteLine("Event count is " + count + " raw data: (" + resultObj.ToString(Formatting.None) + ")");

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