using System;
using System.Collections.Generic;
using System.Text;

namespace zipkin4net.dotnetcore
{
    public class ClientTraceFactory : IClientTraceFactory
    {
        public ClientTrace Create(string serviceName, string rpc)
        {
            return new ClientTrace(serviceName, rpc);
        }
    }
}
