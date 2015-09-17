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
        private readonly long duration;

        public Record(SpanId spanId, DateTime timestamp, IAnnotation annotation, long duration)
        {
            this.spanId = spanId;
            this.timestamp = timestamp;
            this.annotation = annotation;
            this.duration = duration;
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

        public long Duration
        {
            get { return duration; }
        }

        public override string ToString()
        {
            return String.Format("{0} {1}] {2}", Timestamp.ToString(DatetimeFormat), SpanId, Annotation);
        }

    }
}
