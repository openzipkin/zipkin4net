using System.Globalization;
using zipkin4net.Utils;

namespace zipkin4net.Propagation
{
    public static class InjectorHelper
    {
        public static string SerializeTraceId(this ITraceContext spanState)
        {
            var hexTraceId = NumberUtils.EncodeLongToLowerHexString(spanState.TraceId);
            if (spanState.TraceIdHigh == SpanState.NoTraceIdHigh)
            {
                return hexTraceId;
            }
            return NumberUtils.EncodeLongToLowerHexString(spanState.TraceIdHigh) + hexTraceId;
        }

        public static string SerializeSpanId(this ITraceContext spanState)
        {
            return NumberUtils.EncodeLongToLowerHexString(spanState.SpanId);
        }

        public static string SerializeParentSpanId(this ITraceContext spanState)
        {
            return NumberUtils.EncodeLongToLowerHexString(spanState.ParentSpanId.Value);
        }

        public static string SerializeDebugKey(this ITraceContext spanState)
        {
            return ((long)GetFlags(spanState.Sampled, spanState.Debug)).ToString(CultureInfo.InvariantCulture);
        }

        public static string SerializeSampledKey(this ITraceContext spanState)
        {
            return spanState.Sampled.Value ? "1" : "0";
        }

        public static SpanFlags GetFlags(bool? isSampled, bool isDebug)
        {
            var flags = SpanFlags.None;
            if (isSampled.HasValue)
            {
                flags |= SpanFlags.SamplingKnown;
                if (isSampled.Value)
                {
                    flags |= SpanFlags.Sampled;
                }
            }
            if (isDebug)
            {
                flags |= SpanFlags.Debug;
            }
            return flags;
        }
    }
}
