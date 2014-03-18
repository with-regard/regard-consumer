using System;
using Regard.Consumer.Logic.Api;

namespace Regard.Consumer.Logic
{
    /// <summary>
    /// C
    /// </summary>
    public class RegardEvent : IRegardEvent
    {
        private RegardEvent()
        {
            RawData         = null;
            Payload         = null;
            Organization    = null;
            Product         = null;
        }

        /// <summary>
        /// Creates a new event with some raw data
        /// </summary>
        public static IRegardEvent Create(string rawData)
        {
            return new RegardEvent().WithRawData(rawData);
        }

        private IRegardEvent With(Action<RegardEvent> updateEvent)
        {
            var newEvent = new RegardEvent();

            newEvent.RawData        = RawData;
            newEvent.Payload        = Payload;
            newEvent.Organization   = Organization;
            newEvent.Product        = Product;

            updateEvent(newEvent);
            return newEvent;
        }

        /// <summary>
        /// The organization for this event. This should be null if no organisation is known yet, or a JSON encoded string
        /// </summary>
        public string Organization
        {
            get; private set;
        }

        /// <summary>
        /// The payload for this event. This should be null if no payload has been extracted yet, or a JSON encoded string.
        /// </summary>
        public string Payload
        {
            get; private set;
        }

        /// <summary>
        /// The product for this event. This should be null if no organisation is known yet, or a JSON encoded string
        /// </summary>
        public string Product
        {
            get; private set;
        }

        /// <summary>
        /// The raw data for this event. This should be a JSON encoded string, or null if no raw data has been extracted from this event yet.
        /// </summary>
        public string RawData
        {
            get; private set;
        }

        /// <summary>
        /// New event with an organisation
        /// </summary>
        public IRegardEvent WithOrganisation(string data)
        {
            return With((newEvt) => newEvt.Organization = data);
        }

        /// <summary>
        /// New event with a payload
        /// </summary>
        public IRegardEvent WithPayload(string data)
        {
            return With((newEvt) => newEvt.Payload = data);
        }

        /// <summary>
        /// New event with a product
        /// </summary>
        public IRegardEvent WithProduct(string data)
        {
            return With((newEvt) => newEvt.Product = data);
        }

        /// <summary>
        /// Event with some raw data
        /// </summary>
        public IRegardEvent WithRawData(string data)
        { 
            return With((newEvt) => newEvt.RawData = data);
        }
    }
}
