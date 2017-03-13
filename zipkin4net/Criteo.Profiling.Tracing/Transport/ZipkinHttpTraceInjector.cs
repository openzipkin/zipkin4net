using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Net.Http.Headers;

namespace Criteo.Profiling.Tracing.Transport
{
    public class ZipkinHttpTraceInjector : ITraceInjector<NameValueCollection>, ITraceInjector<IDictionary<string, string>>
    {
        public bool Inject(Trace trace, NameValueCollection carrier)
        {
            Set(carrier, trace);
            return true;
        }

        public bool Inject(Trace trace, IDictionary<string, string> carrier)
        {
            Set(carrier, trace);
            return true;
        }

        public bool Inject(Trace trace, HttpHeaders carrier)
        {
            Set(carrier, trace);
            return true;
        }

        // Duplicate code... Don't know any way to avoid this
        public static void Set(NameValueCollection headers, Trace trace)
        {
            var traceId = trace.CurrentSpan;

            headers[ZipkinHttpHeaders.TraceId] = ZipkinHttpHeaders.EncodeLongToHexString(traceId.TraceId);
            headers[ZipkinHttpHeaders.SpanId] = ZipkinHttpHeaders.EncodeLongToHexString(traceId.SpanId);
            if (traceId.ParentSpanId != null)
            {
                // Cannot be null in theory, the root span must have been created on request receive hence further RPC calls are necessary children
                headers[ZipkinHttpHeaders.ParentSpanId] = ZipkinHttpHeaders.EncodeLongToHexString(traceId.ParentSpanId.Value);
            }
            headers[ZipkinHttpHeaders.Flags] = ((long)traceId.Flags).ToString(CultureInfo.InvariantCulture);

            // Add "Sampled" header for compatibility with Finagle
            if (traceId.Flags.HasFlag(SpanFlags.SamplingKnown))
            {
                headers[ZipkinHttpHeaders.Sampled] = traceId.Flags.HasFlag(SpanFlags.Sampled) ? "1" : "0";
            }
        }

        // Duplicate code... Don't know any way to avoid this
        public static void Set(IDictionary<string, string> headers, Trace trace)
        {
            var traceId = trace.CurrentSpan;

            headers[ZipkinHttpHeaders.TraceId] = ZipkinHttpHeaders.EncodeLongToHexString(traceId.TraceId);
            headers[ZipkinHttpHeaders.SpanId] = ZipkinHttpHeaders.EncodeLongToHexString(traceId.SpanId);
            if (traceId.ParentSpanId != null)
            {
                // Cannot be null in theory, the root span must have been created on request receive hence further RPC calls are necessary children
                headers[ZipkinHttpHeaders.ParentSpanId] = ZipkinHttpHeaders.EncodeLongToHexString(traceId.ParentSpanId.Value);
            }
            headers[ZipkinHttpHeaders.Flags] = ((long)traceId.Flags).ToString(CultureInfo.InvariantCulture);

            // Add "Sampled" header for compatibility with Finagle
            if (traceId.Flags.HasFlag(SpanFlags.SamplingKnown))
            {
                headers[ZipkinHttpHeaders.Sampled] = traceId.Flags.HasFlag(SpanFlags.Sampled) ? "1" : "0";
            }
        }

        public static void Set(HttpHeaders headers, Trace trace)
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
