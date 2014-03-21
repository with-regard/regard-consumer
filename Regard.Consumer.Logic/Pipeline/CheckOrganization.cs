using System;
using System.Threading.Tasks;
using Regard.Consumer.Logic.Api;
using Regard.Consumer.Logic.Data;

namespace Regard.Consumer.Logic.Pipeline
{
    /// <summary>
    /// Checks that the organization in an event is valid
    /// </summary>
    public class CheckOrganization : IPipelineStage
    {
        /// <summary>
        /// The name of the partition within the table that contains organisations
        /// </summary>
        private const string c_OrganizationsPartition = "Organizations";

        /// <summary>
        /// The table of organisations
        /// </summary>
        private readonly IFlatTableTarget m_OrganizationTable;

        /// <summary>
        /// Creates a check organization stage
        /// </summary>
        /// <param name="organizationTable">The flat table that contains the organizations. Organizations are put in the 'Organizations' partition within
        /// the table with the organization name as the row key, so this could be a general 'customer details' type table</param>
        public CheckOrganization(IFlatTableTarget organizationTable)
        {
            if (organizationTable == null) throw new ArgumentNullException("organizationTable");

            // Store the table
            m_OrganizationTable = organizationTable;
            
            // Ensure that the 'WithRegard' organisation exists in the table
            var createTask = Task.Run(async () =>
                {
                    var withRegard = await m_OrganizationTable.Find(c_OrganizationsPartition, StorageUtil.SanitiseKey(HealthCheckRoutingStage.c_Organization));
                    if (withRegard == null)
                    {
                        var newWithRegard           = new FlatOrganisationEntity();
                        newWithRegard.PartitionKey  = c_OrganizationsPartition;
                        newWithRegard.RowKey        = StorageUtil.SanitiseKey(HealthCheckRoutingStage.c_Organization);

                        await m_OrganizationTable.Insert(newWithRegard);
                    }
                });

            createTask.Wait();
        }

        /// <summary>
        /// Causes this pipeline stage to process an event
        /// </summary>
        public async Task<IRegardEvent> Process(IRegardEvent input)
        {
            // Drop with an error if the input organization is invalid
            if (string.IsNullOrEmpty(input.Organization()))
            {
                return input.WithError("Invalid organization");
            }

            // Drop with an error if the input organization isn't in the organizations table
            var existingOrganization = await m_OrganizationTable.Find(c_OrganizationsPartition, StorageUtil.SanitiseKey(input.Organization()));

            if (existingOrganization == null)
            {
                return input.WithError("Organization is not registered");
            }

            // TODO: check that this event really belongs to this organization?
            return input;
        }
    }
}
