﻿using System.Collections.Generic;
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
        public AzureTablePipeline(IFlatTableTarget target, string healthCheckSecret)
        {
            var decompose       = new DecomposeStage();
            var checkSize       = new CheckDataSize();
            var checkOrg        = new CheckOrganization();
            var checkProduct    = new CheckProduct();
            var checkUser       = new CheckUser();
            var healthCheck     = new HealthCheckRoutingStage(healthCheckSecret);
            var storeInTable    = new StoreAzureTable(target);

            m_Stages = new IPipelineStage[] {decompose, checkSize, checkOrg, checkProduct, checkUser, healthCheck, storeInTable};
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
