using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Regard.Consumer.SelfTest
{
    public interface ITestStatusListener
    {
        Task TestsFinished(JObject results);
    }
}