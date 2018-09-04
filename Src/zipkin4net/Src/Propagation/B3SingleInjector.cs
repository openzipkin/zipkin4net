namespace zipkin4net.Propagation
{
    internal class B3SingleInjector<C, K> : IInjector<C>
    {
        private readonly B3SinglePropagation<K> _b3Propagation;
        private readonly Setter<C, K> _setter;

        public B3SingleInjector(B3SinglePropagation<K> b3Propagation, Setter<C, K> setter)
        {
            _b3Propagation = b3Propagation;
            _setter = setter;
        }

        public void Inject(ITraceContext traceContext, C carrier)
        {
            _setter(carrier, _b3Propagation.B3Key, B3SingleFormat.WriteB3SingleFormat(traceContext));
        }
    }
}
