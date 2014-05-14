using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Regard.Consumer.SelfTest
{
    /// <summary>
    /// A test is just a task that generates a JObject that says what happened
    /// </summary>
    interface ITest
    {
        /// <summary>
        /// Executes this test and returns the object that will be returned in the result
        /// </summary>
        Task<JObject> Run();

        /// <summary>
        /// The name that should be reported in the JSON results for this test
        /// </summary>
        string Name { get; }
    }
}
