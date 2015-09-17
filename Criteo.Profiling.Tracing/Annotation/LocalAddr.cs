using System;
using System.Net;

namespace Criteo.Profiling.Tracing.Annotation
{
    public sealed class LocalAddr : IAnnotation
    {
        internal LocalAddr(IPEndPoint endPoint)
        {
            this.EndPoint = endPoint;
        }

        public IPEndPoint EndPoint { get; private set; }

        public override string ToString()
        {
            return String.Format("{0} {1}:{2}", GetType().Name, EndPoint.Address, EndPoint.Port);
        }

        public void Accept(IAnnotationVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
