using System.Globalization;
using System.Net.Http.Headers;
using Criteo.Profiling.Tracing;
using Criteo.Profiling.Tracing.Transport;

namespace Criteo.Profiling.Tracing.Middleware
{
    public class ZipkinHttpTraceInjector : ITraceInjector<HttpHeaders>
    {
        public bool Inject(Trace trace, HttpHeaders carrier)
        {
            Set(carrier, trace);
            return true;
        }

        
        private static void Set(HttpHeaders headers, Trace trace)
        {
            var traceId = trace.CurrentSpan;

            headers.Add(ZipkinHttpHeaders.TraceId, ZipkinHttpHeaders.EncodeLongToHexString(traceId.TraceId));
            headers.Add(ZipkinHttpHeaders.SpanId, ZipkinHttpHeaders.EncodeLongToHexString(traceId.SpanId));
            if (traceId.ParentSpanId != null)
            {
                // Cannot be null in theory, the root span must have been created on request receive hence further RPC calls are necessary children
                headers.Add(ZipkinHttpHeaders.ParentSpanId, ZipkinHttpHeaders.EncodeLongToHexString(traceId.ParentSpanId.Value));
            }
            headers.Add(ZipkinHttpHeaders.Flags, ((long)traceId.Flags).ToString(CultureInfo.InvariantCulture));

            // Add "Sampled" header for compatibility with Finagle
            if (traceId.Flags.HasFlag(SpanFlags.SamplingKnown))
            {
                headers.Add(ZipkinHttpHeaders.Sampled, traceId.Flags.HasFlag(SpanFlags.Sampled) ? "1" : "0");
            }
        }

    }
}