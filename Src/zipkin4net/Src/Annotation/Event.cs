using System;

namespace zipkin4net.Annotation
{
    public sealed class Event : IAnnotation
    {
        public string EventName { get; private set; }

        internal Event(string eventName)
        {
            EventName = eventName;
        }

        public override string ToString()
        {
            return string.Format("{0}: {1}", GetType().Name, EventName);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return string.Equals(EventName, ((Event)obj).EventName, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
        {
            return EventName != null ? EventName.GetHashCode() : 0;
        }

        public void Accept(IAnnotationVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
