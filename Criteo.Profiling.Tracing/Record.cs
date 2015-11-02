using System;
using Criteo.Profiling.Tracing.Annotation;

namespace Criteo.Profiling.Tracing
{
    public sealed class Record
    {
        private const string DatetimeFormat = "MMdd HH:mm:ss.fff";

        private readonly SpanId spanId;
        private readonly DateTime timestamp;
        private readonly IAnnotation annotation;

        public Record(SpanId spanId, DateTime timestamp, IAnnotation annotation)
        {
            this.spanId = spanId;
            this.timestamp = timestamp;
            this.annotation = annotation;
        }

        public DateTime Timestamp
        {
            get { return timestamp; }
        }

        public IAnnotation Annotation
        {
            get { return annotation; }
        }

        public SpanId SpanId
        {
            get { return spanId; }
        }

        public override string ToString()
        {
            return String.Format("{0} {1}] {2}", Timestamp.ToString(DatetimeFormat), SpanId, Annotation);
        }

    }
}
