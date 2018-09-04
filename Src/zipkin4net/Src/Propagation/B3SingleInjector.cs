namespace zipkin4net.Propagation
{
    internal class B3SingleInjector<C, K> : IInjector<C>
    {
        private readonly K _b3Key;
        private readonly Setter<C, K> _setter;

        public B3SingleInjector(K b3Key, Setter<C, K> setter)
        {
            _b3Key = b3Key;
            _setter = setter;
        }

        public void Inject(ITraceContext traceContext, C carrier)
        {
            _setter(carrier, _b3Key, B3SingleFormat.WriteB3SingleFormat(traceContext));
        }
    }
}
