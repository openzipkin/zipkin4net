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

        public override string ToString()
        {
            return string.Format("{0}: {1}/{2}", GetType().Name, ServiceName, Endpoint);
        }

        public override void Accept(IAnnotationVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
