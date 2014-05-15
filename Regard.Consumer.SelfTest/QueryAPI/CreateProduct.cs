using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Regard.Consumer.SelfTest.QueryAPI
{
    /// <summary>
    /// Ensures that the query interface is capable of creating new products. The product created by this test is used by other tests, so this generally
    /// needs to execute first.
    /// </summary>
    public class CreateProduct : ITest
    {
        public async Task<JObject> Run()
        {
            // Details for this test
            const string createUrl = "admin/v1/product/create";

            // Generate the payload
            var createPayload = new JObject();
            createPayload["organization"]   = QueryData.OrganizationName;
            createPayload["product"]        = QueryData.ThisSessionProductName;

            // Request that this product be created
            var result = await QueryUtil.RunQuery(createUrl, createPayload, "POST");

            if (result.Item2 != HttpStatusCode.Created)
            {
                return JObject.FromObject(new
                {
                    Result = "Bad response status code",
                    StatusCode = (int) result.Item2
                });
            }

            // There's no data returned for a successful product creation, so just indicate that the project was created OK
            // We could indicate the project name here, but this API is public so we'll avoid that.
            return JObject.FromObject(new
            {
                Result = "Test project created OK",
                StatusCode = (int) result.Item2
            });
        }

        public string Name { get { return "CreateProduct"; } }
    }
}