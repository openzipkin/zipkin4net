namespace zipkin4net.Propagation
{
    internal class B3Extractor<C, K> : IExtractor<C>
    {
        private readonly B3Propagation<K> _b3Propagation;
        private readonly Getter<C, K> _getter;
        private readonly B3SingleExtractor<C, K> _singleExtractor;

        internal B3Extractor(B3Propagation<K> b3Propagation, Getter<C, K> getter)
        {
            _b3Propagation = b3Propagation;
            _getter = getter;
            _singleExtractor = new B3SingleExtractor<C, K>(b3Propagation.B3Key, getter);
        }

        public ITraceContext Extract(C carrier)
        {
            var extracted =_singleExtractor.Extract(carrier);
            if (extracted != null)
            {
                return extracted;
            }

            return ExtractorHelper.TryParseTrace(
                _getter(carrier, _b3Propagation.TraceIdKey),
                _getter(carrier, _b3Propagation.SpanIdKey),
                _getter(carrier, _b3Propagation.ParentSpanIdKey),
                _getter(carrier, _b3Propagation.SampledKey),
                _getter(carrier, _b3Propagation.DebugKey)
            );
        }
    }
}
