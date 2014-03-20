using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Regard.Consumer.Logic
{
    /// <summary>
    /// Generates signatures using shared secretes
    /// </summary>
    public static class SignatureUtil
    {
        /// <summary>
        /// Generates a signature given some data and a shared secret
        /// </summary>
        public static string Signature(string data, string sharedSecret)
        {
            // We can't handle the empty string, so that generates no signature
            if (string.IsNullOrEmpty(data) || string.IsNullOrEmpty(sharedSecret))
            {
                return "";
            }

            // Hash the data and the secret
            var hash        = SHA256.Create();
            var hashBytes   = hash.ComputeHash(Encoding.UTF8.GetBytes(sharedSecret + "--" + data));

            // Generate a result string
            StringBuilder result = new StringBuilder();
            foreach (var byt in hashBytes)
            {
                result.Append(byt.ToString("x2", CultureInfo.InvariantCulture));
            }

            return result.ToString();
        }
    }
}
