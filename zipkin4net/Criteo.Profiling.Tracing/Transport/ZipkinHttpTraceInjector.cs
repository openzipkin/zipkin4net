﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using Criteo.Profiling.Tracing.Utils;

namespace Criteo.Profiling.Tracing.Transport
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
            injector(carrier, ZipkinHttpHeaders.SpanId, NumberUtils.EncodeLongToHexString(spanState.SpanId));
            if (spanState.ParentSpanId != null)
            {
                // Cannot be null in theory, the root span must have been created on request receive hence further RPC calls are necessary children
                injector(carrier, ZipkinHttpHeaders.ParentSpanId, NumberUtils.EncodeLongToHexString(spanState.ParentSpanId.Value));
            }
            injector(carrier, ZipkinHttpHeaders.Flags, ((long)spanState.Flags).ToString(CultureInfo.InvariantCulture));

            // Add "Sampled" header for compatibility with Finagle
            if (spanState.Flags.HasFlag(SpanFlags.SamplingKnown))
            {
                injector(carrier, ZipkinHttpHeaders.Sampled, spanState.Flags.HasFlag(SpanFlags.Sampled) ? "1" : "0");
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

        private static string SerializeTraceId(SpanState spanState)
        {
            var hexTraceId = NumberUtils.EncodeLongToHexString(spanState.TraceId);
            if (spanState.TraceIdHigh == SpanState.NoTraceIdHigh)
            {
                return hexTraceId;
            }
            return NumberUtils.EncodeLongToHexString(spanState.TraceIdHigh) + hexTraceId;
        }
    }
}
