using System;
namespace zipkin4net.Propagation
{
    public static partial class KeyFactories
    {
        public static readonly KeyFactory<string> String = key => key;
    }
}
