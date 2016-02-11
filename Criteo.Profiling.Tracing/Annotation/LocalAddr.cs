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

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return ((LocalAddr)obj).EndPoint.Equals(EndPoint);
        }

        public override int GetHashCode()
        {
            return (EndPoint != null ? EndPoint.GetHashCode() : 0);
        }

        public void Accept(IAnnotationVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
