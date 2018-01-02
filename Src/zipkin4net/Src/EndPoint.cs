using System.Net;

namespace zipkin4net
{
    //TODO internal for now. It's still work in progress
    internal class EndPoint : IEndPoint
    {
        public EndPoint(string serviceName, IPEndPoint ipEndPoint)
        {
            ServiceName = serviceName;
            IpEndPoint = ipEndPoint;
        }

        public string ServiceName { get; private set; }
        public IPEndPoint IpEndPoint { get; private set; }
    }
}