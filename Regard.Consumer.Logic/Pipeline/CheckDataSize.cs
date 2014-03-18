using System.Threading.Tasks;
using Regard.Consumer.Logic.Api;

namespace Regard.Consumer.Logic.Pipeline
{
    /// <summary>
    /// Checks the size of events and rejects anything that's too large
    /// </summary>
    public class CheckDataSize : IPipelineStage
    {
        /// <summary>
        /// The maximum number of characters that can be stored in any one event payload
        /// </summary>
        private const int c_MaxCharacters = 4096;

        /// <summary>
        /// The maximum length of an identifier
        /// </summary>
        /// <remarks>
        /// Partition keys have a limit of 1024 characters, so this is here to try to ensure that the string 'org/product' is never longer than this.
        /// This is checked post-sanitisation, so names using 'bad' characters will have a lower limit.
        /// </remarks>
        private const int c_MaxIdentifier = 256;

        /// <summary>
        /// Causes this pipeline stage to process an event
        /// </summary>
        public async Task<IRegardEvent> Process(IRegardEvent input)
        {
            if (input.Payload() != null && input.Payload().Length > c_MaxCharacters)
            {
                return input.WithError("Event payload is too long");
            }

            var sanitisedProduct    = StorageUtil.SanitiseKey(input.Product());
            var sanitisedOrg        = StorageUtil.SanitiseKey(input.Organization());
            if (sanitisedProduct != null && sanitisedProduct.Length > c_MaxIdentifier)
            {
                return input.WithError("Product name is too long");
            }
            if (sanitisedOrg != null && sanitisedOrg.Length > c_MaxIdentifier)
            {
                return input.WithError("Organization name is too long");
            }

            return input;
        }
    }
}
