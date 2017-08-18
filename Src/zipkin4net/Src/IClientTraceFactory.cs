using System;
using System.Collections.Generic;
using System.Text;

namespace zipkin4net.dotnetcore
{
    public interface IClientTraceFactory
    {
        ClientTrace Create(string serviceName, string rpc);
    }
}
