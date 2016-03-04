using System;
using System.Globalization;

namespace Criteo.Profiling.Tracing
{

    public sealed class SpanState : IEquatable<SpanState>
    {
        public long TraceId { get; private set; }

        public long? ParentSpanId { get; private set; }

        public long SpanId { get; private set; }

        /// <summary>
        /// Bitfield which allows for several options (e.g. debug mode, sampling)
        /// </summary>
        public Flags Flags { get; private set; }

        public SpanState(long traceId, long? parentSpanId, long spanId, Flags flags)
        {
            this.TraceId = traceId;
            this.ParentSpanId = parentSpanId;
            this.SpanId = spanId;
            this.Flags = flags;
        }

        internal void SetSampled()
        {
            this.Flags = this.Flags.SetSampled();
        }

        public bool Equals(SpanState other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return TraceId == other.TraceId && ParentSpanId == other.ParentSpanId && SpanId == other.SpanId;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SpanState)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = TraceId.GetHashCode();
                hashCode = (hashCode * 397) ^ SpanId.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString()
        {
            return String.Format("{0}.{1}<:{2}", TraceId, SpanId, (ParentSpanId.HasValue) ? ParentSpanId.Value.ToString(CultureInfo.InvariantCulture) : "_");
        }

    }
}
