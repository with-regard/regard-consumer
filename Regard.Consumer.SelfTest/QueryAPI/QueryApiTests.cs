namespace Regard.Consumer.SelfTest.QueryAPI
{
    /// <summary>
    /// Class that represents all of the Query API tests
    /// </summary>
    internal class QueryApiTests : MultipleTests
    {
        public QueryApiTests() : base(new ITest[]
        {
            // These tests need to run in a sequence, which is probably a bad thing
            // We'd need to create a new project/query/etc per test if we didn't do this, though, which might be considered too much for checking health in production
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
