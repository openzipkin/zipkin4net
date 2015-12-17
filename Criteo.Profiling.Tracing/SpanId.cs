using System;
using System.Globalization;

namespace Criteo.Profiling.Tracing
{

    public sealed class SpanId : IEquatable<SpanId>
    {
        public long TraceId { get; private set; }

        public long? ParentSpanId { get; private set; }

        public long Id { get; private set; }

        /// <summary>
        /// Bitfield which allows for several options (e.g. debug mode, sampling)
        /// </summary>
        public Flags Flags { get; private set; }

        public SpanId(long traceId, long? parentSpanId, long id, Flags flags)
        {
            this.TraceId = traceId;
            this.ParentSpanId = parentSpanId;
            this.Id = id;
            this.Flags = flags;
        }

        public bool Equals(SpanId other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return TraceId == other.TraceId && ParentSpanId == other.ParentSpanId && Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SpanId)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = TraceId.GetHashCode();
                hashCode = (hashCode * 397) ^ Id.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString()
        {
            return String.Format("{0}.{1}<:{2}", TraceId, Id, (ParentSpanId.HasValue) ? ParentSpanId.Value.ToString(CultureInfo.InvariantCulture) : "_");
        }

    }
}
