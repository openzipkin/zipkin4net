namespace zipkin4net.Propagation
{
    public interface ISetter<C, K>
    {
        void Put(C carrier, K key, string value);
    }
}
