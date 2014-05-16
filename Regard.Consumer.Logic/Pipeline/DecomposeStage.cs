using System;
using System.Data;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Regard.Consumer.Logic.Api;

namespace Regard.Consumer.Logic.Pipeline
{
    /// <summary>
    /// Pipeline stage that decomposes a raw message
    /// </summary>
    public class DecomposeStage : IPipelineStage
    {
        /// <summary>
        /// Causes this pipeline stage to process an event
        /// </summary>
        public async Task<IRegardEvent> Process(IRegardEvent input)
        {
            // Decode the raw data
            JObject inputData;
            try
            {
                inputData = JObject.Parse(input.RawData());
            }
            catch (JsonException e)
            {
                return input.WithError("Raw event is not JSON: " + e.Message);
            }

            // Extract the data from this event
            JToken organization, product, version, payload;

            if (!inputData.TryGetValue("organization", out organization))
            {
                return input.WithError("Event has no organization");
            }

            if (!inputData.TryGetValue("product", out product))
            {
                return input.WithError("Event has no product");
            }

            if (!inputData.TryGetValue("schema_version", out version))
            {
                return input.WithError("Event has no version");
            }

            if (!inputData.TryGetValue("payload", out payload))
            {
                return input.WithError("Event has no payload");
            }

            string adaptedPayload;
            if (!TryGetAdaptedPayload(payload.ToString(), out adaptedPayload))
            {
                return input.WithError("Event doesn't have a valid payload");
            }

            // Check the version number
            if (version.Value<int>() != 0x100)
            {
                return input.WithError("Unknown event version number");
            }

            // TODO: upgrade payload of older events (these don't exist at this point)

            // Create the result
            return input.WithOrganization(organization.ToString())
                        .WithPayload(adaptedPayload)
                        .WithProduct(product.ToString())
                        .WithVersion(version.ToString());
        }

        private static bool TryGetAdaptedPayload(string rawPayload, out string adaptedPayload)
        {
            try
            {
                JObject rawEvent = JObject.Parse(rawPayload);

                JToken dataToken;
                if (rawEvent.TryGetValue("data", StringComparison.OrdinalIgnoreCase, out dataToken))
                {
                    var dataObject = dataToken.Value<JObject>();
                    foreach (var property in dataObject.Properties())
                    {
                        rawEvent.Add(property.Name, property.Value);
                    }

                    rawEvent.Remove("data");
                }

                adaptedPayload = JsonConvert.SerializeObject(rawEvent);
                return true;
            }
            catch (Exception)
            {
                adaptedPayload = null;
                return false;
            }
        }
    }
}
