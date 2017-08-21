using System;

namespace zipkin4net.Annotation
{
    public sealed class ServiceName : IAnnotation
    {
        public string Service { get; private set; }

        internal ServiceName(string service)
        {
            Service = service;
        }

        public override string ToString()
        {
            return string.Format("{0}: {1}", GetType().Name, Service);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return string.Equals(Service, ((ServiceName)obj).Service, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
        {
            return Service != null ? Service.GetHashCode() : 0;
        }

        public void Accept(IAnnotationVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
