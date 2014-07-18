using System.Threading.Tasks;
using Regard.Consumer.Logic.Api;

namespace Regard.Consumer.Logic.Pipeline
{
    /// <summary>
    /// Pipeline stage that notifies an object that an event has been processed
    /// </summary>
    class NotifyEventStage : IPipelineStage
    {
        private readonly IQueryNotifier m_Notifier;

        public NotifyEventStage(IQueryNotifier notifier)
        {
            m_Notifier = notifier;
        }

        public async Task<IRegardEvent> Process(IRegardEvent input)
        {
            var product         = input.Product();
            var organization    = input.Organization();

            if (string.IsNullOrEmpty(product) || string.IsNullOrEmpty(organization))
            {
                return input;
            }

            if (m_Notifier != null)
            {
                m_Notifier.CountEvent(organization, product);
            }

            return input;
        }
    }
}
