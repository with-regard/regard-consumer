namespace Regard.Consumer.SelfTest.QueryAPI
{
    /// <summary>
    /// Class that represents all of the Query API tests
    /// </summary>
    internal class QueryApiTests : MultipleTests
    {
        public QueryApiTests() : base(new[] { new CreateProduct() }, "QueryApiTests")
        {
        }
    }
}