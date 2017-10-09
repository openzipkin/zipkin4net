using System;
using System.Collections.Generic;
using System.Net;

namespace zipkin4net.Annotation
{
    public sealed class MessageAddr : IAnnotation
    {
        public  string ServiceName { get; }
        public IPEndPoint Endpoint { get; }

        internal MessageAddr(string serviceName, IPEndPoint endpoint)
        {
            ServiceName = serviceName;
            Endpoint = endpoint;
        }

        public override string ToString()
        {
            return string.Format("{0}: {1}/{2}", GetType().Name, ServiceName, Endpoint);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            var messageAddr = (MessageAddr)obj;
            return Endpoint.Equals(messageAddr.Endpoint)
                && string.Equals(ServiceName, messageAddr.ServiceName, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
        {
            var hashCode = -2129424941;
            hashCode = hashCode * -1521134295 + ServiceName.GetHashCode();
            hashCode = hashCode * -1521134295 + Endpoint.GetHashCode();
            return hashCode;
        }

        public void Accept(IAnnotationVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
