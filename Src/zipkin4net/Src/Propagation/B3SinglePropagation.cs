using System;
using zipkin4net.Transport;

namespace zipkin4net.Propagation
{
    internal class B3SinglePropagation<K> : IPropagation<K>
    {
        internal readonly K B3Key;

        internal B3SinglePropagation(KeyFactory<K> keyFactory)
        {
            B3Key = keyFactory(ZipkinHttpHeaders.B3);
        }

        public IInjector<C> Injector<C>(Setter<C, K> setter)
        {
            if (setter == null) throw new NullReferenceException("setter == null");
            return new B3SingleInjector<C, K>(this, setter);
        }

        public IExtractor<C> Extractor<C>(Getter<C, K> getter)
        {
            if (getter == null) throw new NullReferenceException("getter == null");
            return new B3SingleExtractor<C, K>(this, getter);
        }
    }
}
