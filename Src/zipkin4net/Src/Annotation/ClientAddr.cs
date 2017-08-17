using System.Net;

namespace Criteo.Profiling.Tracing.Annotation
{
    public sealed class ClientAddr : Addr
    {
        internal ClientAddr(IPEndPoint endpoint)
        : base(endpoint)
        {}

        public override void Accept(IAnnotationVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
