using System.Net;

namespace zipkin4net.Internal.V2
{
    internal struct Endpoint
    {
        public readonly string ServiceName;
        public readonly string Ipv4;

        public Endpoint(string serviceName, string ipv4)
        {
            ServiceName = serviceName;
            Ipv4 = ipv4;
        }
    }
}