using System;

namespace Criteo.Profiling.Tracing.Annotation
{
    public sealed class Event : IAnnotation
    {
        public string EventName { get; private set; }

        internal Event(String eventName)
        {
            this.EventName = eventName;
        }

        public override string ToString()
        {
            return String.Format("{0}: {1}", GetType().Name, EventName);
        }

        public void Accept(IAnnotationVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
