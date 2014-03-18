namespace Regard.Consumer.Logic.Api
{
    /// <summary>
    /// Class that contains extension methods for handling the standard event keys as well as their names
    /// </summary>
    public static class EventKeys
    {
        public const string KeyRawData      = "Regard.RawData";
        public const string KeyProduct      = "Regard.Product";
        public const string KeyOrganization = "Regard.Organization";
        public const string KeyPayload      = "Regard.Payload";
        public const string KeyVersion      = "Regard.Version";
        public const string KeyError        = "Regard.Error";

        public static IRegardEvent WithRawData(this IRegardEvent oldEvent, string rawData)
        {
            return oldEvent.With(KeyRawData, rawData);
        }

        public static string RawData(this IRegardEvent evt)
        {
            return evt[KeyRawData];
        }

        public static IRegardEvent WithProduct(this IRegardEvent oldEvent, string product)
        {
            return oldEvent.With(KeyProduct, product);
        }
        public static string Product(this IRegardEvent evt)
        {
            return evt[KeyProduct];
        }

        public static IRegardEvent WithOrganization(this IRegardEvent oldEvent, string organization)
        {
            return oldEvent.With(KeyOrganization, organization);
        }

        public static string Organization(this IRegardEvent evt)
        {
            return evt[KeyOrganization];
        }

        public static IRegardEvent WithPayload(this IRegardEvent oldEvent, string payload)
        {
            return oldEvent.With(KeyPayload, payload);
        }

        public static string Payload(this IRegardEvent evt)
        {
            return evt[KeyPayload];
        }

        public static IRegardEvent WithError(this IRegardEvent oldEvent, string error)
        {
            return oldEvent.With(KeyError, error);
        }

        public static string Error(this IRegardEvent evt)
        {
            return evt[KeyError];
        }

        public static IRegardEvent WithVersion(this IRegardEvent oldEvent, string version)
        {
            return oldEvent.With(KeyVersion, version);
        }

        public static string Version(this IRegardEvent evt)
        {
            return evt[KeyVersion];
        }
    }
}
