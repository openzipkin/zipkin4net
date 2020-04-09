using System;
using zipkin4net.Transport;
using zipkin4net.Utils;

namespace zipkin4net.Propagation
{
    internal static class ExtractorHelper
    {
        private const int TraceId64BitsSerializationLength = 16;

        //Internal due to backward compatibility of ZipkinHttpTraceExtractor. Once ZipkinHttpTraceExtractor
        //is removed, we can set the visibility to private
        internal static ITraceContext TryParseTrace(string encodedTraceId, string encodedSpanId, string encodedParentSpanId, string sampledStr, string flagsStr)
        {
            if (string.IsNullOrWhiteSpace(encodedTraceId)
                || string.IsNullOrWhiteSpace(encodedSpanId))
            {
                return default(SpanState);
            }

            try
            {
                var traceIdHigh = ExtractTraceIdHigh(encodedTraceId);
                var traceId = ExtractTraceId(encodedTraceId);
                var spanId = NumberUtils.DecodeHexString(encodedSpanId);
                var parentSpanId = string.IsNullOrWhiteSpace(encodedParentSpanId) ? null : (long?)NumberUtils.DecodeHexString(encodedParentSpanId);
                var flags = ZipkinHttpHeaders.ParseFlagsHeader(flagsStr);
                var sampled = ZipkinHttpHeaders.ParseSampledHeader(sampledStr);

                if (sampled != null)
                {
                    // When "sampled" header exists, it overrides any existing flags
                    flags = SpanFlags.SamplingKnown;
                    if (sampled.Value)
                    {
                        flags |= SpanFlags.Sampled;
                    }
                }
                else
                {
                    if ((flags & SpanFlags.SamplingKnown) == SpanFlags.SamplingKnown)
                    {
                        sampled = (flags & SpanFlags.Sampled) == SpanFlags.Sampled;
                    }
                }

                return new SpanState(traceIdHigh, traceId, parentSpanId, spanId, sampled, (flags & SpanFlags.Debug) == SpanFlags.Debug);
            }
            catch (Exception ex)
            {
                TraceManager.Logger.LogWarning("Couldn't parse trace context. Trace is ignored. Message:" + ex.Message);
            }

            return default(SpanState);
        }

        /// <summary>
        /// Extracts traceIdHigh. Detects if present and then decode the first 16 bytes
        /// </summary>
        private static long ExtractTraceIdHigh(string encodedTraceId)
        {
            if (encodedTraceId.Length <= TraceId64BitsSerializationLength)
            {
                return SpanState.NoTraceIdHigh;
            }
            var traceIdHighLength = encodedTraceId.Length - TraceId64BitsSerializationLength;
            var traceIdHighStr = encodedTraceId.Substring(0, traceIdHighLength);
            return NumberUtils.DecodeHexString(traceIdHighStr);
        }

        /// <summary>
        /// Extracts traceId. Detects if present and then decode the last 16 bytes
        /// </summary>
        private static long ExtractTraceId(string encodedTraceId)
        {
            var traceIdLength = TraceId64BitsSerializationLength;
            if (encodedTraceId.Length <= TraceId64BitsSerializationLength)
            {
                traceIdLength = encodedTraceId.Length;
            }
            var traceIdStartIndex = encodedTraceId.Length - traceIdLength;
            return NumberUtils.DecodeHexString(encodedTraceId.Substring(traceIdStartIndex, traceIdLength));
        }
    }
}
