using Microsoft.WindowsAzure.Storage.Table;

namespace Regard.Consumer.Logic.Data
{
    /// <summary>
    /// Flat representation of an event in the table
    /// </summary>
    public class FlatEventEntity : TableEntity
    {
        /// <summary>
        /// The organisation that this event belongs to
        /// </summary>
        public string Organization { get; set; }

        /// <summary>
        /// The product that this event belongs to
        /// </summary>
        public string Product { get; set; }

        /// <summary>
        /// The event payload
        /// </summary>
        public string Payload { get; set; }
    }
}
