using System.Net;

namespace zipkin4net
{
    //TODO internal for now. It's still work in progress
    internal interface IEndPoint
    {
        string ServiceName { get; }
        
        IPEndPoint IpEndPoint { get; }
    }
}