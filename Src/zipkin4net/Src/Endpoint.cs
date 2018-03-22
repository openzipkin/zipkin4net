using System.Net;

namespace zipkin4net
{
    internal struct Endpoint
    {
        public readonly string ServiceName;
        public readonly IPEndPoint IpEndPoint;

        public Endpoint(string serviceName, IPEndPoint ipEndPoint)
        {
            ServiceName = serviceName;
            IpEndPoint = ipEndPoint;
        }
    }
}