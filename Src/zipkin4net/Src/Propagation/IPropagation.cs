namespace zipkin4net.Propagation
{
    public interface IPropagation<K>
    {
        IInjector<C> Injector<C>(ISetter<C, K> setter);

        IExtractor<C> Extractor<C>(IGetter<C, K> getter);
    }
}
