using System;
using System.Collections.Generic;
using Regard.Consumer.Logic.Api;

namespace Regard.Consumer.Logic
{
    /// <summary>
    /// Class representing an event
    /// </summary>
    public class RegardEvent : IRegardEvent
    {
        /// <summary>
        /// The events in this object
        /// </summary>
        private readonly Dictionary<string, string> m_Values = new Dictionary<string, string>();

        private RegardEvent()
        {
        }

        /// <summary>
        /// Creates a new event
        /// </summary>
        public static IRegardEvent Create(string rawData)
        {
            return new RegardEvent().WithRawData(rawData);
        }

        /// <summary>
        /// Returns the value of a particular key
        /// </summary>
        public string this[string key]
        {
            get
            {
                string result;
                if (m_Values.TryGetValue(key, out result))
                {
                    return result;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Returns an event identical to this one, except that the specified key is given the specified value
        /// </summary>
        public IRegardEvent With(string key, string value)
        {
            var newEvent = new RegardEvent();
            foreach (var kvPair in m_Values)
            {
                newEvent.m_Values.Add(kvPair.Key, kvPair.Value);
            }
            newEvent.m_Values[key] = value;

            return newEvent;
        }
    }
}
