namespace zipkin4net.Propagation
{
    internal class B3SingleExtractor<C, K> : IExtractor<C>
    {
        private readonly B3SinglePropagation<K> _b3SinglePropagation;
        private readonly Getter<C, K> _getter;

        private const int TraceId64BitsSerializationLength = 16;

        internal B3SingleExtractor(B3SinglePropagation<K> b3SinglePropagation, Getter<C, K> getter)
        {
            _b3SinglePropagation = b3SinglePropagation;
            _getter = getter;
        }

        public ITraceContext Extract(C carrier)
        {
            return B3SingleFormat.ParseB3SingleFormat(
                _getter(carrier, _b3SinglePropagation.B3Key)
            );
        }
    }
}