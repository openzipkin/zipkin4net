using Criteo.Profiling.Tracing.Annotation;
using System;

namespace Criteo.Profiling.Tracing
{
    public class ClientTrace : IDisposable
    {
        public Trace Trace { get; private set; }

        public ClientTrace(string serviceName, string rpc)
        {
            if (Trace.Current != null) {
              Trace = Trace.Current.Child();
            }

            Trace.Record(Annotations.ClientSend());
            Trace.Record(Annotations.ServiceName(serviceName));
            Trace.Record(Annotations.Rpc(rpc));
        }

        public void AddAnnotation(IAnnotation annotation)
        {
            Trace.Record(annotation);
        }

        public void Dispose()
        {
            Trace.Record(Annotations.ClientRecv());
        }
    }
}