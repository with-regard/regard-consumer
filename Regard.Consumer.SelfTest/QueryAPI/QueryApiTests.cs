namespace Regard.Consumer.SelfTest.QueryAPI
{
    /// <summary>
    /// Class that represents all of the Query API tests
    /// </summary>
    internal class QueryApiTests : MultipleTests
    {
        public QueryApiTests() : base(new ITest[]
        {
            new CanCreateTestProduct()
            , new CanOptInDefaultUser()
            , new CanCreateQuery()
            , new QueryInitiallyReturnsNoResults()
            , new QueryReturnsDataOnceThereAreMatchingEvents()
        
        }, "QueryApiTests")
        {
        }
    }
}
