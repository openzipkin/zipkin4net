using System;
using zipkin4net.Transport;

namespace zipkin4net.Propagation
{
    public delegate K KeyFactory<K>(string key);
    
    internal class B3Propagation<K> : IPropagation<K>
    {
        internal readonly K TraceIdKey;
        internal readonly K SpanIdKey;
        internal readonly K ParentSpanIdKey;
        internal readonly K SampledKey;
        internal readonly K DebugKey;

        internal B3Propagation(KeyFactory<K> keyFactory)
        {
            TraceIdKey = keyFactory(ZipkinHttpHeaders.TraceId);
            SpanIdKey = keyFactory(ZipkinHttpHeaders.SpanId);
            ParentSpanIdKey = keyFactory(ZipkinHttpHeaders.ParentSpanId);
            SampledKey = keyFactory(ZipkinHttpHeaders.Sampled);
            DebugKey = keyFactory(ZipkinHttpHeaders.Flags);
        }

        public IInjector<C> Injector<C>(Setter<C, K> setter)
        {
            if (setter == null) throw new NullReferenceException("setter == null");
            return new B3Injector<C, K>(this, setter);
        }

        public IExtractor<C> Extractor<C>(Getter<C, K> getter)
        {
            if (getter == null) throw new NullReferenceException("getter == null");
            return new B3Extractor<C, K>(this, getter);
        }
    }
}
