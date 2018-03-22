using System;
using System.Linq;
using System.Net;
using zipkin4net.Tracers.Zipkin.Thrift;

namespace zipkin4net.Tracers.Zipkin
{
    public class BinaryAnnotation
    {
        public class Endpoint
        {
            public string ServiceName { get; private set; }
            public IPEndPoint IPEndPoint { get; private set; }

            internal Endpoint(string serviceName, IPEndPoint ipEndPoint)
            {
                ServiceName = serviceName;
                IPEndPoint = ipEndPoint;
            }
        }

        public AnnotationType AnnotationType { get; private set; }

        public byte[] Value { get; private set; }

        public string Key { get; private set; }

        public DateTime Timestamp { get; private set; }

        public Endpoint Host { get; private set; }

        internal BinaryAnnotation(string key, byte[] value, AnnotationType annotationType, DateTime timestamp,
            string serviceName, IPEndPoint endPoint)
        {
            Key = key;
            Value = value;
            AnnotationType = annotationType;
            Timestamp = timestamp;
            Host = CreateEndPoint(serviceName, endPoint);
        }

        private Endpoint CreateEndPoint(string serviceName, IPEndPoint endPoint)
        {
            if (endPoint != null)
            {
                return new Endpoint(serviceName, endPoint);
            }

            return null;
        }

        public override string ToString()
        {
            return string.Format("BinAnn: type={0} key={1}", AnnotationType, Key);
        }

        private bool Equals(BinaryAnnotation other)
        {
            return AnnotationType == other.AnnotationType
                   && Value.SequenceEqual(other.Value)
                   && string.Equals(Key, other.Key)
                   && Timestamp.Equals(other.Timestamp);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((BinaryAnnotation) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int) AnnotationType;
                hashCode = (hashCode * 397) ^ (Value != null ? Value.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Key != null ? Key.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Timestamp.GetHashCode();
                return hashCode;
            }
        }
    }
}