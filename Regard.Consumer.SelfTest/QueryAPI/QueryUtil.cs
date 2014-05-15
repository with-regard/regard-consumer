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
        /// Calls the query API
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
                var payloadStream = request.GetRequestStream();
                payloadStream.Write(payloadBytes, 0, payloadBytes.Length);
            }

            // Perform the request
            using (var response = (HttpWebResponse) await request.GetResponseAsync())
            {
                // Process the result
                var statusCode = response.StatusCode;
                JObject resultData = null;

                // Read the response data
                if (response.ContentType == "application/json" || response.ContentType == "text/json")
                {
                    using (var responseStream = response.GetResponseStream())
                    {
                        if (responseStream != null)
                        { 
                            var streamReader = new StreamReader(responseStream, Encoding.GetEncoding(response.ContentEncoding));
                            var shouldBeJson = await streamReader.ReadToEndAsync();

                            // Convert to JSON
                            try
                            {
                                resultData = JObject.Parse(shouldBeJson);
                            }
                            catch (JsonException e)
                            {
                                Trace.TraceError("Response is not valid JSON");
                                resultData = null;
                            }
                        }
                    }
                }

                // Return as a tuple
                return new Tuple<JObject, HttpStatusCode>(resultData, statusCode);
            }
        }
    }
}