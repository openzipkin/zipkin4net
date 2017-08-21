using System;
using System.Net;

namespace zipkin4net.Annotation
{
    public sealed class LocalAddr : IAnnotation
    {
        internal LocalAddr(IPEndPoint endPoint)
        {
            EndPoint = endPoint;
        }

        public IPEndPoint EndPoint { get; private set; }

        public override string ToString()
        {
            return string.Format("{0} {1}:{2}", GetType().Name, EndPoint.Address, EndPoint.Port);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return ((LocalAddr)obj).EndPoint.Equals(EndPoint);
        }

        public override int GetHashCode()
        {
            return EndPoint != null ? EndPoint.GetHashCode() : 0;
        }

        public void Accept(IAnnotationVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
