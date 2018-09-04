namespace zipkin4net.Propagation
{
    internal class B3SingleExtractor<C, K> : IExtractor<C>
    {
        private readonly K _b3Key;
        private readonly Getter<C, K> _getter;

        private const int TraceId64BitsSerializationLength = 16;

        internal B3SingleExtractor(K b3Key, Getter<C, K> getter)
        {
            _b3Key = b3Key;
            _getter = getter;
        }

        public ITraceContext Extract(C carrier)
        {
            return B3SingleFormat.ParseB3SingleFormat(
                _getter(carrier, _b3Key)
            );
        }
    }
}