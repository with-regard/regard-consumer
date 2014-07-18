namespace Regard.Consumer.Logic.Api
{
    /// <summary>
    /// Object whose job it is to pass on notifications that the queries for a particular product are out of date
    /// </summary>
    public interface IQueryNotifier
    {
        /// <summary>
        /// Counts one or more events against a particular product
        /// </summary>
        void CountEvent(string organization, string product, int numEvents = 1);
    }
}
