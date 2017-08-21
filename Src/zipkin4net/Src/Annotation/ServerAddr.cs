using System.Net;

namespace zipkin4net.Annotation
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
