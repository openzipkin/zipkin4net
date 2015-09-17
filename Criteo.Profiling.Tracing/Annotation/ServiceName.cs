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

        public void Accept(IAnnotationVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
