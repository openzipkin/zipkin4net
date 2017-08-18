using System;
using System.Collections.Generic;
using System.Text;

namespace zipkin4net.dotnetcore
{
    public class ServerTraceFactory : IServerTraceFactory
    {
        public ServerTrace Create(string serviceName, string rpc)
        {
            return new ServerTrace(serviceName,rpc);
        }
    }
}
