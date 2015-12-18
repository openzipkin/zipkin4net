using System;
using Criteo.Profiling.Tracing.Annotation;

namespace Criteo.Profiling.Tracing
{
    public sealed class Record
    {
        private const string DatetimeFormat = "MMdd HH:mm:ss.fff";

        private readonly SpanId _spanId;
        private readonly DateTime _timestamp;
        private readonly IAnnotation _annotation;

        public Record(SpanId spanId, DateTime timestamp, IAnnotation annotation)
        {
            _spanId = spanId;
            _timestamp = timestamp;
            _annotation = annotation;
        }

        public DateTime Timestamp
        {
            get { return _timestamp; }
        }

        public IAnnotation Annotation
        {
            get { return _annotation; }
        }

        public SpanId SpanId
        {
            get { return _spanId; }
        }

        public override string ToString()
        {
            return String.Format("{0} {1}] {2}", Timestamp.ToString(DatetimeFormat), SpanId, Annotation);
        }

    }
}
