using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Criteo.Profiling.Tracing.Transport
{
    public class ZipkinHttpTraceExtractor : ITraceExtractor<NameValueCollection>, ITraceExtractor<IDictionary<string, string>>
    {
        public bool TryExtract(NameValueCollection carrier, out Trace trace)
        {
            return TryParseTrace(carrier[ZipkinHttpHeaders.TraceId],
               carrier[ZipkinHttpHeaders.SpanId],
               carrier[ZipkinHttpHeaders.ParentSpanId],
               carrier[ZipkinHttpHeaders.Sampled],
               carrier[ZipkinHttpHeaders.Flags],
               out trace);
        }

        public bool TryExtract(IDictionary<string, string> carrier, out Trace trace)
        {
            string encodedTraceId, encodedSpanId;

            if (carrier.TryGetValue(ZipkinHttpHeaders.TraceId, out encodedTraceId)
                && carrier.TryGetValue(ZipkinHttpHeaders.SpanId, out encodedSpanId))
            {
                string flagsStr, sampledStr, encodedParentSpanId;
                carrier.TryGetValue(ZipkinHttpHeaders.Flags, out flagsStr);
                carrier.TryGetValue(ZipkinHttpHeaders.Sampled, out sampledStr);
                carrier.TryGetValue(ZipkinHttpHeaders.ParentSpanId, out encodedParentSpanId);

                return TryParseTrace(encodedTraceId, encodedSpanId, encodedParentSpanId, sampledStr, flagsStr, out trace);
            }

            trace = default(Trace);
            return false;
        }

        internal static bool TryParseTrace(string encodedTraceId, string encodedSpanId, string encodedParentSpanId, string sampledStr, string flagsStr, out Trace trace)
        {
            if (string.IsNullOrWhiteSpace(encodedTraceId)
                || string.IsNullOrWhiteSpace(encodedSpanId))
            {
                trace = default(Trace);
                return false;
            }

            try
            {
                var traceId = ZipkinHttpHeaders.DecodeHexString(encodedTraceId);
                var spanId = ZipkinHttpHeaders.DecodeHexString(encodedSpanId);
                var parentSpanId = string.IsNullOrWhiteSpace(encodedParentSpanId) ? null : (long?)ZipkinHttpHeaders.DecodeHexString(encodedParentSpanId);
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
