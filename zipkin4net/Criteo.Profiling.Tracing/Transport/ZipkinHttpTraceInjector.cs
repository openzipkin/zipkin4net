using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;

namespace Criteo.Profiling.Tracing.Transport
{
    /**
     * Inject B3 headers into HTTP headers.
     */
    public class ZipkinHttpTraceInjector : ITraceInjector<NameValueCollection>, ITraceInjector<IDictionary<string, string>>, ITraceInjector
    {
        public bool Inject<TE>(Trace trace, TE carrier, Action<TE, string, string> injector)
        {
            var traceId = trace.CurrentSpan;

            injector(carrier, ZipkinHttpHeaders.TraceId, ZipkinHttpHeaders.EncodeLongToHexString(traceId.TraceId));
            injector(carrier, ZipkinHttpHeaders.SpanId, ZipkinHttpHeaders.EncodeLongToHexString(traceId.SpanId));
            if (traceId.ParentSpanId != null)
            {
                // Cannot be null in theory, the root span must have been created on request receive hence further RPC calls are necessary children
                injector(carrier, ZipkinHttpHeaders.ParentSpanId, ZipkinHttpHeaders.EncodeLongToHexString(traceId.ParentSpanId.Value));
            }
            injector(carrier, ZipkinHttpHeaders.Flags, ((long)traceId.Flags).ToString(CultureInfo.InvariantCulture));

            // Add "Sampled" header for compatibility with Finagle
            if (traceId.Flags.HasFlag(SpanFlags.SamplingKnown))
            {
                injector(carrier, ZipkinHttpHeaders.Sampled, traceId.Flags.HasFlag(SpanFlags.Sampled) ? "1" : "0");
            }
            return true;
        }
        
        public bool Inject(Trace trace, NameValueCollection carrier)
        {
            return Inject(trace, carrier, (c, key, value) => c[key] = value);
        }

        public bool Inject(Trace trace, IDictionary<string, string> carrier)
        {
            return Inject(trace, carrier, (c, key, value) => c[key] = value);
        }
    }
}
