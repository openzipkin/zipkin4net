namespace zipkin4net.Propagation
{
    public interface IKeyFactory<K>
    {
        K Create(string name);
    }
}
