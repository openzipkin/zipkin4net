using System;
using System.Collections.Generic;
using System.Globalization;

namespace zipkin4net
{
    [Serializable]
    public sealed class SpanState : IEquatable<SpanState>, ITraceContext
    {
        public long TraceIdHigh { get; private set; }

        public long TraceId { get; private set; }

        public long? ParentSpanId { get; private set; }

        public long SpanId { get; private set; }

        internal const long NoTraceIdHigh = 0;

        [Obsolete("Please use Sampled method instead")]
        public SamplingStatus SamplingStatus
        {
            get
            {
                if (Sampled.HasValue)
                {
                    return Sampled.Value ? SamplingStatus.Sampled : SamplingStatus.NotSampled;
                }

                return SamplingStatus.NoDecision;
            }
        }

        /// <summary>
        /// Allows for several options (e.g. debug mode, sampling)
        /// </summary>
        [Obsolete("Please use Sampled and Debug method instead")]
        public SpanFlags Flags { get; private set; }


        [Obsolete("Please use SpanState(long traceId, long? parentSpanId, long spanId, bool? isSampled, bool isDebug)")]
        public SpanState(long traceId, long? parentSpanId, long spanId, SpanFlags flags)
            : this(NoTraceIdHigh, traceId, parentSpanId, spanId, flags)
        {
        }

        [Obsolete(
            "Please use SpanState(long traceIdHigh, long traceId, long? parentSpanId, long spanId, bool? isSampled, bool isDebug)")]
        public SpanState(long traceIdHigh, long traceId, long? parentSpanId, long spanId, SpanFlags flags)
            : this(traceIdHigh, traceId, parentSpanId, spanId,
                flags.HasFlag(SpanFlags.SamplingKnown) ? flags.HasFlag(SpanFlags.Sampled) : (bool?) null,
                flags.HasFlag(SpanFlags.Debug))
        {
            Flags = flags;
        }

        public SpanState(long traceId, long? parentSpanId, long spanId, bool? isSampled, bool isDebug)
            : this(NoTraceIdHigh, traceId, parentSpanId, spanId, isSampled, isDebug)
        {
        }

        public SpanState(long traceIdHigh, long traceId, long? parentSpanId, long spanId, bool? isSampled, bool isDebug)
            : this(traceIdHigh, traceId, parentSpanId, spanId, isSampled, isDebug, new List<object>())
        {
        }

        public SpanState(long traceIdHigh, long traceId, long? parentSpanId, long spanId, bool? isSampled, bool isDebug,
            List<object> extra)
        {
            TraceIdHigh = traceIdHigh;
            TraceId = traceId;
            ParentSpanId = parentSpanId;
            SpanId = spanId;
            Sampled = isSampled;
            Debug = isDebug;
#pragma warning disable 618
            Flags = GetFlagsForBackwardCompatibility(isSampled, isDebug);
#pragma warning restore 618
            Extra = extra;
        }

        internal SpanState(ITraceContext traceContext, List<object> extra)
            : this(traceContext.TraceIdHigh, traceContext.TraceId, traceContext.ParentSpanId, traceContext.SpanId,
                traceContext.Sampled, traceContext.Debug, extra)
        {
        }

        private static SpanFlags GetFlagsForBackwardCompatibility(bool? isSampled, bool isDebug)
        {
            var flags = SpanFlags.None;
            if (isSampled.HasValue)
            {
                flags |= SpanFlags.SamplingKnown & SpanFlags.Sampled;
            }

            if (isDebug)
            {
                flags |= SpanFlags.Debug;
            }

            return flags;
        }

        public bool Equals(SpanState other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return TraceIdHigh == other.TraceIdHigh
                   && TraceId == other.TraceId
                   && ParentSpanId == other.ParentSpanId
                   && SpanId == other.SpanId;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((SpanState) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = TraceIdHigh.GetHashCode();
                hashCode = (hashCode * 397) ^ TraceId.GetHashCode();
                hashCode = (hashCode * 397) ^ ParentSpanId.GetHashCode();
                hashCode = (hashCode * 397) ^ SpanId.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString()
        {
            return string.Format("{0}{1}.{2}<:{3}",
                TraceIdHigh == SpanState.NoTraceIdHigh ? "" : TraceIdHigh.ToString(), TraceId, SpanId,
                ParentSpanId.HasValue ? ParentSpanId.Value.ToString(CultureInfo.InvariantCulture) : "_");
        }

        public bool? Sampled { get; private set; }

        public bool Debug { get; private set; }

        public List<object> Extra { get; private set; }
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