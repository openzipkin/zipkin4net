using System;
using Criteo.Profiling.Tracing.Annotation;

namespace Criteo.Profiling.Tracing
{
    public sealed class Record
    {
        private const string DatetimeFormat = "MMdd HH:mm:ss.fff";

        private readonly SpanState _spanState;
        private readonly DateTime _timestamp;
        private readonly IAnnotation _annotation;

        public Record(SpanState spanState, DateTime timestamp, IAnnotation annotation)
        {
            _spanState = spanState;
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

        public SpanState SpanState
        {
            get { return _spanState; }
        }

        public override string ToString()
        {
            return String.Format("{0} {1}] {2}", Timestamp.ToString(DatetimeFormat), SpanState, Annotation);
        }

    }
}
