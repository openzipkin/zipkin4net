using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Criteo.Profiling.Tracing.Utils;

namespace Criteo.Profiling.Tracing.Transport
{
    /**
     * Extract B3 headers from HTTP headers.
     */
    public class ZipkinHttpTraceExtractor : ITraceExtractor<NameValueCollection>, ITraceExtractor<IDictionary<string, string>>, ITraceExtractor
    {
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
            return TryExtract(carrier, (c, key) => {
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
                var traceId = NumberUtils.DecodeHexString(encodedTraceId);
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


                var state = new SpanState(traceId, parentSpanId, spanId, flags);
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

    }
}
