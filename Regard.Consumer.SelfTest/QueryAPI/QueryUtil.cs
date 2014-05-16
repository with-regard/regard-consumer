using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Regard.Consumer.SelfTest.QueryAPI
{
    /// <summary>
    /// Utilities for calling the query library
    /// </summary>
    public static class QueryUtil
    {
        /// <summary>
        /// Creates valid event data ready to be sent to the endpoint from payload information
        /// </summary>
        public static JObject BuildValidEvent(JObject payload)
        {
            // Format the event as a new session event
            JObject realEvent = new JObject();

            realEvent["user-id"] = QueryData.TestUserId;
            realEvent["new-session"] = true;
            realEvent["session-id"] = Guid.NewGuid().ToString();
            realEvent["data"] = payload;

            return realEvent;
        }

        /// <summary>
        /// Sends an event relating to the test product
        /// </summary>
        public static async Task<HttpStatusCode> SendEvent(JObject payload)
        {
            var realEvent = BuildValidEvent(payload);
            var targetUrl = QueryData.IngestionEndpointUrl + "/track/v1/" + QueryData.OrganizationName + "/" + QueryData.ThisSessionProductName + "/event";

            // Convert to binary
            Trace.WriteLine("Sending event: " + realEvent);
            Trace.WriteLine("Target URL: targetUrl");
            var payloadBytes = Encoding.UTF8.GetBytes(realEvent.ToString());

            // Send to the service
            var request = WebRequest.Create(new Uri(targetUrl));
            request.Method = "POST";
            request.ContentType = "application/json";
            request.ContentLength = payloadBytes.Length;

            var payloadStream = await request.GetRequestStreamAsync();
            payloadStream.Write(payloadBytes, 0, payloadBytes.Length);
            payloadStream.Close();

            using (var response = (HttpWebResponse)await request.GetResponseAsync())
            {
                Trace.WriteLine("Event response code: " + response.StatusCode);
                return response.StatusCode;
            }
        }

        /// <summary>
        /// Calls the query API, returning the JSON object that represents the result and the status code
        /// </summary>
        public static async Task<Tuple<JObject, HttpStatusCode>> RunQuery(string path, JObject data, string verb)
        {
            Trace.WriteLine("Requesting " + path);

            // Create the web request
            var request = WebRequest.Create(new Uri(new Uri(QueryData.QueryEndPointUrl), path));

            request.Method = verb;

            if (data != null)
            {
                // Generate the payload
                var payloadBytes        = Encoding.UTF8.GetBytes(data.ToString());
                request.ContentType     = "application/json";
                request.ContentLength   = payloadBytes.Length;

                // Write to the request
                var payloadStream = await request.GetRequestStreamAsync();
                payloadStream.Write(payloadBytes, 0, payloadBytes.Length);
                payloadStream.Close();
            }

            // Perform the request
            using (var response = (HttpWebResponse) await request.GetResponseAsync())
            {
                // Process the result
                var statusCode = response.StatusCode;
                JObject resultData = null;

                // Read the response data
                if (response.ContentType.StartsWith("application/json") || response.ContentType.StartsWith("text/json"))
                {
                    using (var responseStream = response.GetResponseStream())
                    {
                        if (responseStream != null)
                        { 
                            var streamReader = new StreamReader(responseStream, Encoding.UTF8);
                            var shouldBeJson = await streamReader.ReadToEndAsync();

                            // Convert to JSON
                            try
                            {
                                resultData = JObject.Parse(shouldBeJson);
                            }
                            catch (JsonException)
                            {
                                Trace.TraceError("Response is not valid JSON");
                                resultData = null;
                            }
                        }
                    }
                }
                else
                { 
                    Trace.WriteLine("Response content-type is " + response.ContentType);
                }

                // Return as a tuple
                return new Tuple<JObject, HttpStatusCode>(resultData, statusCode);
            }
        }
    }
}