using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Regard.Query.Serializable;

namespace Regard.Consumer.SelfTest.QueryAPI
{
    public class CanCreateQuery : ITest
    {
        public async Task<JObject> Run()
        {
            // Only evenst where 'event-type' = 'test'
            var serializer = new SerializableQueryBuilder(null);
            var query = serializer.AllEvents();
            query = serializer.Only(query, "event-type", "test");

            // Convert into a format where we can send it to the service
            var jsonQuery = query.ToJson();

            JObject queryRequest = new JObject();
            queryRequest["query-name"] = "test";
            queryRequest["query-definition"] = jsonQuery;

            // Register the query
            var queryUrl = "product/v1/" + QueryData.OrganizationName + "/" + QueryData.ThisSessionProductName + "/register-query";

            var response = await QueryUtil.RunQuery(queryUrl, queryRequest, "POST");

            if (response.Item2 != HttpStatusCode.Created)
            {
                return JObject.FromObject(new
                {
                    Error = "Bad response status code",
                    StatusCode = (int)response.Item2
                });
            }

            return JObject.FromObject(new
            {
                Result = "Query created OK",
                StatusCode = (int) response.Item2
            });
        }

        public string Name { get { return "CanCreateQuery"; } }
    }
}