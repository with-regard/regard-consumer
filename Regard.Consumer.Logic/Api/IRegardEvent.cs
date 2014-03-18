namespace Regard.Consumer.Logic.Api
{
    /// <summary>
    /// Fluent, immutable API representing an event in transit
    /// </summary>
    public interface IRegardEvent
    {
        /// <summary>
        /// Returns an event identical to this one, except that the specified key is given the specified value
        /// </summary>
        IRegardEvent With(string key, string value);

        /// <summary>
        /// Returns the value of a particular key. Keys that are not set are returned as null.
        /// </summary>
        string this[string key] { get; }
    }
}
