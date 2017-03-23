using Criteo.Profiling.Tracing.Annotation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Criteo.Profiling.Tracing
{
    public class ServerTrace : IDisposable
    {
        public Trace Trace
        {
            get
            {
                return Trace.Current;
            }
        }

        public ServerTrace(string serviceName, string rpc)
        {
            Trace.Record(Annotations.ServerRecv());
            Trace.Record(Annotations.ServiceName(serviceName));
            Trace.Record(Annotations.Rpc(rpc));
        }

        public void AddAnnotation(IAnnotation annotation)
        {
            Trace.Record(annotation);
        }

        public void Dispose()
        {
            Trace.Record(Annotations.ServerSend());
        }
    }
}
