using System;

namespace zipkin4net.Internal.V2
{
    internal struct Annotation
    {
        /// <summary>
        /// Microseconds from epoch.
        /// 
        /// <p>This value should be set directly by instrumentation, using the most precise value possible.
        /// </summary>
        public readonly DateTime Timestamp;

        /// <summary>
        /// Usually a short tag indicating an event, like <code>cache.miss</code> or <code>error</code> 
        /// </summary>
        public readonly string Value;

        public Annotation(DateTime timestamp, string value)
        {
            Timestamp = timestamp;
            Value = value;
        }
    }
}
