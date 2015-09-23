using System;
using Criteo.Profiling.Tracing.Tracers.Zipkin.Thrift;

namespace Criteo.Profiling.Tracing.Tracers.Zipkin
{
    internal class BinaryAnnotation
    {
        public AnnotationType AnnotationType { get; private set; }

        public byte[] Value { get; private set; }

        public string Key { get; private set; }

        public BinaryAnnotation(string key, byte[] value, AnnotationType annotationType)
        {
            this.Key = key;
            this.Value = value;
            this.AnnotationType = annotationType;
        }

        public Thrift.BinaryAnnotation ToThrift()
        {
            return new Thrift.BinaryAnnotation { Annotation_type = AnnotationType, Key = Key, Value = Value };
        }

        public override string ToString()
        {
            return String.Format("BinAnn: type={0} key={1}", AnnotationType, Key);
        }

    }
}
