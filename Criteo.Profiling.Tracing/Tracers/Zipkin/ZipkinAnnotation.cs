using System;
using Criteo.Profiling.Tracing.Utils;

namespace Criteo.Profiling.Tracing.Tracers.Zipkin
{
    internal class ZipkinAnnotation
    {
        private readonly DateTime _timestamp;

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
                Timestamp = TimeUtils.ToUnixTimestamp(_timestamp),
                Value = this.Value
            };

            return thriftAnn;
        }

        public override string ToString()
        {
            return String.Format("ZipkinAnn: ts={0} val={1}", TimeUtils.ToUnixTimestamp(_timestamp), Value);
        }


    }
}
