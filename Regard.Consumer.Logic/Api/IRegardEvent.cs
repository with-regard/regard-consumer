namespace Regard.Consumer.Logic.Api
{
    /// <summary>
    /// Fluent, immutable API representing an event in transit
    /// </summary>
    public interface IRegardEvent
    {
        /// <summary>
        /// The raw data for this event. This should be a JSON encoded string, or null if no raw data has been extracted from this event yet.
        /// </summary>
        string RawData { get; }

        /// <summary>
        /// The payload for this event. This should be null if no payload has been extracted yet, or a JSON encoded string.
        /// </summary>
        string Payload { get; }

        /// <summary>
        /// The organization for this event. This should be null if no organisation is known yet, or a JSON encoded string
        /// </summary>
        string Organization { get; }

        /// <summary>
        /// The product for this event. This should be null if no organisation is known yet, or a JSON encoded string
        /// </summary>
        string Product { get; }

        /// <summary>
        /// Event with some raw data
        /// </summary>
        IRegardEvent WithRawData(string data);

        /// <summary>
        /// New event with a payload
        /// </summary>
        IRegardEvent WithPayload(string data);

        /// <summary>
        /// New event with an organisation
        /// </summary>
        IRegardEvent WithOrganisation(string data);

        /// <summary>
        /// New event with a product
        /// </summary>
        IRegardEvent WithProduct(string data);
    }
}
