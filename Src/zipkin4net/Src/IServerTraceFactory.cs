using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;

namespace Criteo.Profiling.Tracing.dotnetcore
{
    public interface IServerTraceFactory
    {
        ServerTrace Create(string serviceName, string rpc);
    }
}
