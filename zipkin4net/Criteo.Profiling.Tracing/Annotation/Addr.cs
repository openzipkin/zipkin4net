using System.Net;

namespace Criteo.Profiling.Tracing.Annotation
{
    public abstract class Addr : IAnnotation
    {
        public IPEndPoint Endpoint { get; private set; }

        internal Addr(IPEndPoint endpoint)
        {
            Endpoint = endpoint;
        }

        public override string ToString()
        {
            return string.Format("{0}: {1}", GetType().Name, Endpoint);
        }

        public abstract void Accept(IAnnotationVisitor visitor);
    }
}
