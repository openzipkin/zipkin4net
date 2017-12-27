using System;
using zipkin4net.Transport;

namespace zipkin4net.Propagation
{
    internal class B3Propagation<K> : IPropagation<K>
    {
        internal readonly K TraceIdKey;
        internal readonly K SpanIdKey;
        internal readonly K ParentSpanIdKey;
        internal readonly K SampledKey;
        internal readonly K DebugKey;

        internal B3Propagation(IKeyFactory<K> keyFactory)
        {
            TraceIdKey = keyFactory.Create(ZipkinHttpHeaders.TraceId);
            SpanIdKey = keyFactory.Create(ZipkinHttpHeaders.SpanId);
            ParentSpanIdKey = keyFactory.Create(ZipkinHttpHeaders.ParentSpanId);
            SampledKey = keyFactory.Create(ZipkinHttpHeaders.Sampled);
            DebugKey = keyFactory.Create(ZipkinHttpHeaders.Flags);
        }

        public IInjector<C> Injector<C>(ISetter<C, K> setter)
        {
            if (setter == null) throw new NullReferenceException("setter == null");
            return new B3Injector<C, K>(this, setter);
        }

        public IExtractor<C> Extractor<C>(IGetter<C, K> getter)
        {
            if (getter == null) throw new NullReferenceException("getter == null");
            return new B3Extractor<C, K>(this, getter);
        }
    }
}
