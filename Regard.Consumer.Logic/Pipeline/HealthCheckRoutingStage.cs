using System.Diagnostics;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Regard.Consumer.Logic.Api;

namespace Regard.Consumer.Logic.Pipeline
{
    /// <summary>
    /// Sets the row key from the payload for packets matching a health check pattern
    /// </summary>
    public class HealthCheckRoutingStage : IPipelineStage
    {
        /// <summary>
        /// The organization that should be used for health check events
        /// </summary>
        public const string c_Organization = "RedGateSoftware";

        /// <summary>
        /// The product that should be used for health check events
        /// </summary>
        public const string c_Product = "HealthCheck";

        /// <summary>
        /// The shared secret string, used to verify real health check packets
        /// </summary>
        private readonly string m_SharedSecret;

        public HealthCheckRoutingStage(string sharedSecret)
        {
            m_SharedSecret = sharedSecret;
        }

        /// <summary>
        /// Causes this pipeline stage to process an event
        /// </summary>
        public async Task<IRegardEvent> Process(IRegardEvent input)
        {
            // Packet must be for the health check project
            if (input.Organization() != c_Organization || input.Product() != c_Product)
            {
                return input;
            }

            // Ignore messages without a payload
            if (string.IsNullOrEmpty(input.Payload()))
            {
                return input;
            }

            // Write to the log that we got this far
            Trace.WriteLine("Detected HealthCheck event");

            // Decode the payload
            try
            {
                var decodedPayload = JObject.Parse(input.Payload());

                // There should be a row key token and a row key signature
                // Note: it's possible to replay these packets, but not change the row key without the shared secret
                JToken rowKeyToken, rowKeySignatureToken;

                if (!decodedPayload.TryGetValue("rowkey", out rowKeyToken))
                {
                    Trace.TraceError("HealthCheck: event missing row key");
                    return input;
                }

                if (!decodedPayload.TryGetValue("rowkeysignature", out rowKeySignatureToken))
                {
                    Trace.TraceError("HealthCheck: missing row key signature");
                    return input;
                }

                // Verify the signature
                var rowKey          = rowKeyToken.Value<string>();
                var rowKeySignature = rowKeySignatureToken.Value<string>();

                if (string.IsNullOrEmpty(rowKey))
                {
                    Trace.TraceError("HealthCheck: row key is empty");
                    return input;
                }

                if (string.IsNullOrEmpty(rowKeySignature))
                {
                    Trace.TraceError("HealthCheck: row key signature is empty");
                    return input;
                }

                var realSignature = SignatureUtil.Signature(rowKey, m_SharedSecret);
                if (string.IsNullOrEmpty(realSignature))
                {
                    Trace.TraceError("HealthCheck: could not verify signature");
                    return input;
                }

                if (realSignature != rowKeySignature)
                {
                    Trace.TraceError("HealthCheck: incorrect row key signature");
                    return input;
                }

                // Looks OK
                Trace.WriteLine("HealthCheck: storing with row key {0}", rowKey);

                // Set the row key in the result
                var result = input.With(EventKeys.KeyRowKey, rowKey);
                return result;
            }
            catch (JsonException e)
            {
                // Report as an error
                Trace.TraceError("HealthCheck: Could not decode event: {0}", e);

                // Leave unchanged if we can't decode the payload
                return input;
            }
        }
    }
}
