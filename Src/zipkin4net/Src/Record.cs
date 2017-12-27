using System;
using zipkin4net.Annotation;

namespace zipkin4net
{
    public sealed class Record
    {
        private const string DatetimeFormat = "MMdd HH:mm:ss.fff";

        private readonly ITraceContext _spanState;
        private readonly DateTime _timestamp;
        private readonly IAnnotation _annotation;

        public Record(ITraceContext spanState, DateTime timestamp, IAnnotation annotation)
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

        public ITraceContext SpanState
        {
            get { return _spanState; }
        }

        public override string ToString()
        {
            return string.Format("{0} {1}] {2}", Timestamp.ToString(DatetimeFormat), SpanState, Annotation);
        }

    }
}
