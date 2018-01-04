using System;

namespace zipkin4net.Propagation
{
    public delegate void Setter<C, K>(C carrier, K key, string value);
    public delegate string Getter<C, K>(C carrier, K key);
    
    public interface IPropagation<K>
    {
        IInjector<C> Injector<C>(Setter<C, K> setter);

        IExtractor<C> Extractor<C>(Getter<C, K> getter);
    }
}
