using System.Diagnostics;
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
                var httpResponseMessage = await httpClient.PostAsync(c_SlackUrl, new StringContent(JsonConvert.SerializeObject(new
                                                                                                                               {
                                                                                                                                   text = results
                                                                                                                               })));

                if (!httpResponseMessage.IsSuccessStatusCode)
                    Trace.TraceWarning("Unable to send the message to the Slack API. " + httpResponseMessage.RequestMessage);
            }
        }
    }
}