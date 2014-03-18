using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Regard.Consumer.Logic.Api;

namespace Regard.Consumer.Logic.Pipeline
{
    /// <summary>
    /// Pipeline stage that decomposes a raw message
    /// </summary>
    class DecomposeStage : IPipelineStage
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
                inputData = JObject.Parse(input.RawData);
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

            if (!inputData.TryGetValue("version", out version))
            {
                return input.WithError("Event has no version");
            }

            if (!inputData.TryGetValue("payload", out payload))
            {
                return input.WithError("Event has no organization");
            }

            // Check the version number
            if (version.Value<int>() != 0x100)
            {
                return input.WithError("Unknown event version number");
            }

            // TODO: upgrade payload of older events (these don't exist at this point)

            // Create the result
            return input.WithOrganization(organization.ToString())
                        .WithPayload(payload.ToString())
                        .WithProduct(product.ToString());
        }
    }
}
