using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Regard.Consumer.SelfTest.QueryAPI
{
    public class CanOptInDefaultUser : ITest
    {
        public async Task<JObject> Run()
        {
            Trace.WriteLine("Opt in user ID: " + QueryData.TestUserId);

            var optinPayload = new JObject();
            var optinUrl = "product/v1/" + QueryData.OrganizationName + "/" + QueryData.ThisSessionProductName + "/users/" + QueryData.TestUserId + "/opt-in";
            var result = await QueryUtil.RunQuery(optinUrl, optinPayload, "POST");

            if (result.Item2 != HttpStatusCode.OK)
            {
                return JObject.FromObject(new
                {
                    Result = "Bad response status code",
                    StatusCode = (int) result.Item2
                });
            }

            return JObject.FromObject(new
            {
                Result = "Test user opted in",
                StatusCode = (int) result.Item2
            });
        }

        public string Name { get { return "CanOptInDefaultUser"; } }
    }
}