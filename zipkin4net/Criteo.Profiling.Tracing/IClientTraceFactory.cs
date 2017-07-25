using System;
using System.Collections.Generic;
using System.Text;

namespace Criteo.Profiling.Tracing.dotnetcore
{
    public interface IClientTraceFactory
    {
        ClientTrace Create(string serviceName, string rpc);
    }
}
