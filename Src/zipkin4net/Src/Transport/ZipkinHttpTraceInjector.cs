using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using zipkin4net.Utils;

namespace zipkin4net.Transport
{
    /**
     * Inject B3 headers into HTTP headers.
     */
    public class ZipkinHttpTraceInjector : ITraceInjector<NameValueCollection>, ITraceInjector<IDictionary<string, string>>, ITraceInjector
    {
        public bool Inject<TE>(Trace trace, TE carrier, Action<TE, string, string> injector)
        {
            var spanState = trace.CurrentSpan;

            injector(carrier, ZipkinHttpHeaders.TraceId, SerializeTraceId(spanState));
            injector(carrier, ZipkinHttpHeaders.SpanId, NumberUtils.EncodeLongToLowerHexString(spanState.SpanId));
            if (spanState.ParentSpanId != null)
            {
                // Cannot be null in theory, the root span must have been created on request receive hence further RPC calls are necessary children
                injector(carrier, ZipkinHttpHeaders.ParentSpanId, NumberUtils.EncodeLongToLowerHexString(spanState.ParentSpanId.Value));
            }
            injector(carrier, ZipkinHttpHeaders.Flags, ((long)GetFlags(spanState.Sampled, spanState.Debug)).ToString(CultureInfo.InvariantCulture));

            // Add "Sampled" header for compatibility with Finagle
            if (spanState.Sampled.HasValue)
            {
                injector(carrier, ZipkinHttpHeaders.Sampled, spanState.Sampled.Value ? "1" : "0");
            }
            return true;
        }

        private static SpanFlags GetFlags(bool? isSampled, bool isDebug)
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

        public bool Inject(Trace trace, NameValueCollection carrier)
        {
            return Inject(trace, carrier, (c, key, value) => c[key] = value);
        }

        public bool Inject(Trace trace, IDictionary<string, string> carrier)
        {
            return Inject(trace, carrier, (c, key, value) => c[key] = value);
        }

        private static string SerializeTraceId(ITraceContext spanState)
        {
            var hexTraceId = NumberUtils.EncodeLongToLowerHexString(spanState.TraceId);
            if (spanState.TraceIdHigh == SpanState.NoTraceIdHigh)
            {
                return hexTraceId;
            }
            return NumberUtils.EncodeLongToLowerHexString(spanState.TraceIdHigh) + hexTraceId;
        }
    }
}
