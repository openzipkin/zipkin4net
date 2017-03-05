using System;
using Criteo.Profiling.Tracing.Tracers.Zipkin.Thrift;

namespace Criteo.Profiling.Tracing.Tracers.Zipkin
{
    internal class BinaryAnnotation
    {
        public AnnotationType AnnotationType { get; private set; }

        public byte[] Value { get; private set; }

        public string Key { get; private set; }

        public DateTime Timestamp { get; private set; }

        internal BinaryAnnotation(string key, byte[] value, AnnotationType annotationType, DateTime timestamp)
        {
            Key = key;
            Value = value;
            AnnotationType = annotationType;
            Timestamp = timestamp;
        }

        public override string ToString()
        {
            return string.Format("BinAnn: type={0} key={1}", AnnotationType, Key);
        }

    }
}
