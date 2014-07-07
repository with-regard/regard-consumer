using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Regard.Consumer.SelfTest
{
    public class SlackTestStatusListener : ITestStatusListener
    {
        private const string c_SlackUrl = "https://regard.slack.com/services/hooks/incoming-webhook?token=A0OkEziCzApOIFFJ9R4KcOCD";

        public async Task TestsFinished(JObject results)
        {
            using (var httpClient = new HttpClient())
            {
                await httpClient.PostAsync(c_SlackUrl, new StringContent(JsonConvert.SerializeObject(new
                                                                                                     {
                                                                                                         text = results
                                                                                                     })));
            }
        }
    }
}