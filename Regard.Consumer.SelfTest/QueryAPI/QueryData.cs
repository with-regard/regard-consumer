using System;
using System.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace Regard.Consumer.SelfTest.QueryAPI
{
    /// <summary>
    /// Data that describes what resources the query API tests will use
    /// </summary>
    public class QueryData
    {
        static QueryData()
        {
            ThisSessionProductName = Guid.NewGuid().ToString();
            TestUserId = Guid.NewGuid().ToString();
        }
        
        /// <summary>
        /// The name of the organization that the test projects will be created under
        /// </summary>
        public static string OrganizationName { get { return "WithRegard"; } }

        /// <summary>
        /// The internal endpoint where the query API should reside
        /// </summary>
        public static string QueryEndPointUrl 
        {
            get
            {
                var endpoint = RoleEnvironment.Roles["Regard.Query.Internal.WebAPI"].Instances[0].InstanceEndpoints["Regard.Query.WebAPI"];
                return "http://" + endpoint.IPEndpoint.Address + ":" + endpoint.IPEndpoint.Port;
            } 
        }

        /// <summary>
        /// The name of the product that this test session should create. This should be unique for every session, so will indicate a
        /// product that doesn't exist (these products are always created under the WithRegard namespace and are not deleted later on
        /// by this framework, though it's always safe to do so manually)
        /// </summary>
        public static string ThisSessionProductName { get; private set; }

        /// <summary>
        /// The UID of the test user
        /// </summary>
        public static string TestUserId { get; private set; }
    }
}