using System;

namespace Criteo.Profiling.Tracing.Tracers.Zipkin
{
    internal class ZipkinAnnotation
    {
        private readonly DateTime _timestamp;

        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        internal string Value { get; private set; }

        public ZipkinAnnotation(DateTime timestamp, string value)
        {
            _timestamp = timestamp;
            Value = value;
        }

        public Thrift.Annotation ToThrift()
        {
            var thriftAnn = new Thrift.Annotation()
            {
                Timestamp = ToUnixTimestamp(_timestamp),
                Value = this.Value
            };

            return thriftAnn;
        }

        public override string ToString()
        {
            return String.Format("ZipkinAnn: ts={0} val={1}", ToUnixTimestamp(_timestamp), Value);
        }

        /// <summary>
        /// Create a UNIX timestamp from a UTC date time. Time is expressed in microseconds and not seconds.
        /// </summary>
        /// <see href="https://en.wikipedia.org/wiki/Unix_time"/>
        /// <param name="utcDateTime"></param>
        /// <returns></returns>
        internal static long ToUnixTimestamp(DateTime utcDateTime)
        {
            return (long)(utcDateTime.Subtract(Epoch).TotalMilliseconds * 1000L);
        }
    }
}
