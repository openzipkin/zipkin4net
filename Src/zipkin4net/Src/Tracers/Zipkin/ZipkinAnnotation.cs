using System;
using zipkin4net.Utils;

namespace zipkin4net.Tracers.Zipkin
{
    public class ZipkinAnnotation
    {
        public string Value { get; private set; }

        public DateTime Timestamp { get; private set; }

        public ZipkinAnnotation(DateTime timestamp, string value)
        {
            Timestamp = timestamp;
            Value = value;
        }

        public override string ToString()
        {
            return string.Format("ZipkinAnn: ts={0} val={1}", Timestamp.ToUnixTimestamp(), Value);
        }

    }
}
