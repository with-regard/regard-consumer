using System.Collections.Generic;
using Regard.Consumer.Logic.Api;

namespace Regard.Consumer.Logic.Pipeline
{
    /// <summary>
    /// Fairly basic pipeline that moves events from the service bus into Azure table storage
    /// </summary>
    public class AzureTablePipeline : IPipeline
    {
        private readonly IPipelineStage[] m_Stages;

        /// <summary>
        /// Creates a new table pipeline
        /// </summary>
        public AzureTablePipeline(string storageConnectionString, string storageTableName)
        {
            var decompose       = new DecomposeStage();
            var checkOrg        = new CheckOrganization();
            var checkProduct    = new CheckProduct();
            var checkUser       = new CheckUser();
            var storeInTable    = new StoreAzureTable(storageConnectionString, storageTableName);

            m_Stages = new IPipelineStage[] {decompose, checkOrg, checkProduct, checkUser, storeInTable};
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
