using System;
using System.Globalization;

namespace Criteo.Profiling.Tracing
{
    [Serializable]
    public sealed class SpanState : IEquatable<SpanState>
    {
        public long TraceId { get; private set; }

        public long? ParentSpanId { get; private set; }

        public long SpanId { get; private set; }

        public SamplingStatus SamplingStatus
        {
            get
            {
                if (!Flags.HasFlag(SpanFlags.SamplingKnown)) return SamplingStatus.NoDecision;

                return Flags.HasFlag(SpanFlags.Sampled) ? SamplingStatus.Sampled : SamplingStatus.NotSampled;
            }
        }

        /// <summary>
        /// Allows for several options (e.g. debug mode, sampling)
        /// </summary>
        public SpanFlags Flags { get; private set; }

        public SpanState(long traceId, long? parentSpanId, long spanId, SpanFlags flags)
        {
            TraceId = traceId;
            ParentSpanId = parentSpanId;
            SpanId = spanId;
            Flags = flags;
        }

        /// <summary>
        /// Indicate that this span is relevant and should be sent.
        /// </summary>
        internal void SetSampled()
        {
            Flags = Flags | SpanFlags.SamplingKnown | SpanFlags.Sampled;
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
            if (obj.GetType() != GetType()) return false;
            return Equals((SpanState)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = TraceId.GetHashCode();
                hashCode = (hashCode * 397) ^ SpanId.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString()
        {
            return string.Format("{0}.{1}<:{2}", TraceId, SpanId, ParentSpanId.HasValue ? ParentSpanId.Value.ToString(CultureInfo.InvariantCulture) : "_");
        }

    }

    [Flags]
    public enum SpanFlags
    {
        None = 0,
        Debug = 1 << 0,
        SamplingKnown = 1 << 1,
        Sampled = 1 << 2
    }

    public enum SamplingStatus
    {
        NoDecision = 0,
        Sampled = 1,
        NotSampled = 2,
    }
}
