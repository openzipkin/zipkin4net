using System;
namespace zipkin4net.Propagation
{
    public static partial class KeyFactories
    {
        public static readonly IKeyFactory<string> String = new StringKeyFactory();

        private class StringKeyFactory : IKeyFactory<string>
        {
            public string Create(string name)
            {
                return name;
            }
        }
    }
}
