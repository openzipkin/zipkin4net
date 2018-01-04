using System.Globalization;
using zipkin4net.Utils;

namespace zipkin4net.Propagation
{
    internal class B3Injector<C, K> : IInjector<C>
    {
        private readonly B3Propagation<K> _b3Propagation;
        private readonly Setter<C, K> _setter;

        public B3Injector(B3Propagation<K> b3Propagation, Setter<C, K> setter)
        {
            _b3Propagation = b3Propagation;
            _setter = setter;
        }

        public void Inject(ITraceContext traceContext, C carrier)
        {
            _setter(carrier, _b3Propagation.TraceIdKey, SerializeTraceId(traceContext));
            _setter(carrier, _b3Propagation.SpanIdKey, NumberUtils.EncodeLongToLowerHexString(traceContext.SpanId));
            if (traceContext.ParentSpanId != null)
            {
                // Cannot be null in theory, the root span must have been created on request receive hence further RPC calls are necessary children
                _setter(carrier, _b3Propagation.ParentSpanIdKey, NumberUtils.EncodeLongToLowerHexString(traceContext.ParentSpanId.Value));
            }
            _setter(carrier, _b3Propagation.DebugKey, ((long)GetFlags(traceContext.Sampled, traceContext.Debug)).ToString(CultureInfo.InvariantCulture));

            // Add "Sampled" header for compatibility with Finagle
            if (traceContext.Sampled.HasValue)
            {
                _setter(carrier, _b3Propagation.SampledKey, traceContext.Sampled.Value ? "1" : "0");
            }
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
    }
}
