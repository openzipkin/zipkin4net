using System;
using zipkin4net.Transport;
using zipkin4net.Utils;

namespace zipkin4net.Propagation
{
    internal class B3Extractor<C, K> : IExtractor<C>
    {
        private readonly B3Propagation<K> _b3Propagation;
        private readonly Getter<C, K> _getter;

        private const int TraceId64BitsSerializationLength = 16;

        internal B3Extractor(B3Propagation<K> b3Propagation, Getter<C, K> getter)
        {
            _b3Propagation = b3Propagation;
            _getter = getter;
        }

        public ITraceContext Extract(C carrier)
        {
            return TryParseTrace(
                _getter(carrier, _b3Propagation.TraceIdKey),
                _getter(carrier, _b3Propagation.SpanIdKey),
                _getter(carrier, _b3Propagation.ParentSpanIdKey),
                _getter(carrier, _b3Propagation.SampledKey),
                _getter(carrier, _b3Propagation.DebugKey)
            );
        }

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
                        flags = flags | SpanFlags.Sampled;
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