namespace zipkin4net.Propagation
{
    public static partial class Propagations
    {
        public static readonly IPropagation<string> B3String = new B3Propagation<string>(KeyFactories.String);
    }
}
