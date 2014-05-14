using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Regard.Consumer.SelfTest.QueryAPI
{
    /// <summary>
    /// Ensures that the query interface is capable of creating new products. The product created by this test is used by other tests, so this generally
    /// needs to execute first.
    /// </summary>
    public class CreateProduct : ITest
    {
        public async Task<JObject> Run()
        {
            // Just return an empty JObject for now
            return new JObject();
        }

        public string Name { get { return "CreateProduct"; } }
    }
}