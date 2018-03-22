using System;
using zipkin4net.Utils;

namespace zipkin4net.Tracers.Zipkin
{
    public class ZipkinAnnotation
    {
        public string Value { get; private set; }

        public DateTime Timestamp { get; private set; }

        public ZipkinAnnotation(DateTime timestamp, string value)
        {
            Timestamp = timestamp;
            Value = value;
        }

        public override string ToString()
        {
            return string.Format("ZipkinAnn: ts={0} val={1}", Timestamp.ToUnixTimestamp(), Value);
        }

        private bool Equals(ZipkinAnnotation other)
        {
            return string.Equals(Value, other.Value) && Timestamp.Equals(other.Timestamp);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ZipkinAnnotation) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Value != null ? Value.GetHashCode() : 0) * 397) ^ Timestamp.GetHashCode();
            }
        }
    }
}
