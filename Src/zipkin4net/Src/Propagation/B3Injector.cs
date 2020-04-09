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
            _setter(carrier, _b3Propagation.TraceIdKey, traceContext.SerializeTraceId());
            _setter(carrier, _b3Propagation.SpanIdKey, traceContext.SerializeSpanId());
            if (traceContext.ParentSpanId != null)
            {
                // Cannot be null in theory, the root span must have been created on request receive hence further RPC calls are necessary children
                _setter(carrier, _b3Propagation.ParentSpanIdKey, traceContext.SerializeParentSpanId());
            }
            _setter(carrier, _b3Propagation.DebugKey, traceContext.SerializeDebugKey());

            // Add "Sampled" header for compatibility with Finagle
            if (traceContext.Sampled.HasValue)
            {
                _setter(carrier, _b3Propagation.SampledKey, traceContext.SerializeSampledKey());
            }
        }
    }
}
