using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using zipkin4net.Utils;

namespace zipkin4net.Transport
{
    /**
     * Extract B3 headers from HTTP headers.
     */
    public class ZipkinHttpTraceExtractor : ITraceExtractor<NameValueCollection>, ITraceExtractor<IDictionary<string, string>>, ITraceExtractor
    {
        private const int traceId64BitsSerializationLength = 16;

        public bool TryExtract<TE>(TE carrier, Func<TE, string, string> extractor, out Trace trace)
        {
            return TryParseTrace(
                extractor(carrier, ZipkinHttpHeaders.TraceId),
                extractor(carrier, ZipkinHttpHeaders.SpanId),
                extractor(carrier, ZipkinHttpHeaders.ParentSpanId),
                extractor(carrier, ZipkinHttpHeaders.Sampled),
                extractor(carrier, ZipkinHttpHeaders.Flags),
                out trace
            );
        }

        public bool TryExtract(NameValueCollection carrier, out Trace trace)
        {
            return TryExtract(carrier, (c, key) => c[key], out trace);
        }

        public bool TryExtract(IDictionary<string, string> carrier, out Trace trace)
        {
            return TryExtract(carrier, (c, key) =>
            {
                string value;
                return c.TryGetValue(key, out value) ? value : null;
            }, out trace);
        }

        public static bool TryParseTrace(string encodedTraceId, string encodedSpanId, string encodedParentSpanId, string sampledStr, string flagsStr, out Trace trace)
        {
            if (string.IsNullOrWhiteSpace(encodedTraceId)
                || string.IsNullOrWhiteSpace(encodedSpanId))
            {
                trace = default(Trace);
                return false;
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

                var state = new SpanState(traceIdHigh, traceId, parentSpanId, spanId, sampled, (flags & SpanFlags.Debug) == SpanFlags.Debug);
                trace = Trace.CreateFromId(state);
                return true;
            }
            catch (Exception ex)
            {
                TraceManager.Logger.LogWarning("Couldn't parse trace context. Trace is ignored. Message:" + ex.Message);
            }

            trace = default(Trace);
            return false;
        }

        /// <summary>
        /// Extracts traceIdHigh. Detects if present and then decode the first 16 bytes
        /// </summary>
        private static long ExtractTraceIdHigh(string encodedTraceId)
        {
            if (encodedTraceId.Length <= traceId64BitsSerializationLength)
            {
                return SpanState.NoTraceIdHigh;
            }
            var traceIdHighLength = encodedTraceId.Length - traceId64BitsSerializationLength;
            var traceIdHighStr = encodedTraceId.Substring(0, traceIdHighLength);
            return NumberUtils.DecodeHexString(traceIdHighStr);
        }

        /// <summary>
        /// Extracts traceId. Detects if present and then decode the last 16 bytes
        /// </summary>
        private static long ExtractTraceId(string encodedTraceId)
        {
            var traceIdLength = traceId64BitsSerializationLength;
            if (encodedTraceId.Length <= traceId64BitsSerializationLength)
            {
                traceIdLength = encodedTraceId.Length;
            }
            var traceIdStartIndex = encodedTraceId.Length - traceIdLength;
            return NumberUtils.DecodeHexString(encodedTraceId.Substring(traceIdStartIndex, traceIdLength));
        }
    }
}
