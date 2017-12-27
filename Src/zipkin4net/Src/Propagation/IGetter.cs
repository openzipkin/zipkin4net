namespace zipkin4net.Propagation
{
    public interface IGetter<C, K>
    {
        string Get(C carrier, K key);
    }
}
