using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;

namespace zipkin4net.dotnetcore
{
    public interface IServerTraceFactory
    {
        ServerTrace Create(string serviceName, string rpc);
    }
}
