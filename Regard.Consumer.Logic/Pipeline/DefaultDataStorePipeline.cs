using System.Collections.Generic;
using Regard.Consumer.Logic.Api;

namespace Regard.Consumer.Logic.Pipeline
{
    /// <summary>
    /// Pipeline that stores data in the Azure table as well as the default Data Storage event recorder
    /// </summary>
    public class DefaultDataStorePipeline : IPipeline
    {
        private readonly IPipelineStage[] m_Stages;

        /// <summary>
        /// Creates a new table pipeline
        /// </summary>
        public DefaultDataStorePipeline(IFlatTableTarget eventTable, IFlatTableTarget customerTable, IQueryNotifier notifier, string healthCheckSecret)
        {
            var decompose       = new DecomposeStage();
            var checkSize       = new CheckDataSize();
            var checkOrg        = new CheckOrganization(customerTable);
            var checkProduct    = new CheckProduct();
            var checkUser       = new CheckUser();
            var checkEvent      = new CheckEvent();
            var healthCheck     = new HealthCheckRoutingStage(healthCheckSecret);
            var storeInTable    = new StoreAzureTable(eventTable);
            var storeForQuery   = new StoreQueryEngine();
            var notify          = new NotifyEventStage(notifier);

            m_Stages = new IPipelineStage[] { decompose, checkSize, checkOrg, checkProduct, checkUser, checkEvent, healthCheck, storeInTable, storeForQuery, notify };
        }

        /// <summary>
        /// The stages in this pipeline
        /// </summary>
        public IEnumerable<IPipelineStage> Stages
        {
            get
            {
                return m_Stages;
            }
        }
    }
}
