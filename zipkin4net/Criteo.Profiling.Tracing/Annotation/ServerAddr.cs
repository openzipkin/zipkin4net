using System.Net;

namespace Criteo.Profiling.Tracing.Annotation
{
    public sealed class ServerAddr : Addr
    {
        public string ServiceName { get; private set; }
        internal ServerAddr(string serviceName, IPEndPoint endpoint)
        : base(endpoint)
        {
            ServiceName = serviceName;
        }

        public override void Accept(IAnnotationVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
