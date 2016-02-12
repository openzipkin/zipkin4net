using System;

namespace Criteo.Profiling.Tracing.Annotation
{
    public sealed class ServiceName : IAnnotation
    {
        public string Service { get; private set; }

        internal ServiceName(String service)
        {
            this.Service = service;
        }

        public override string ToString()
        {
            return String.Format("{0}: {1}", GetType().Name, Service);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return string.Equals(Service, ((ServiceName)obj).Service, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
        {
            return (Service != null ? Service.GetHashCode() : 0);
        }

        public void Accept(IAnnotationVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
