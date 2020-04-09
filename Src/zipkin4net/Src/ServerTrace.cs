using System;

namespace zipkin4net
{
    public class ServerTrace : BaseStandardTrace, IDisposable
    {
        public override Trace Trace
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

        public void Dispose()
        {
            Trace.Record(Annotations.ServerSend());
        }
    }
}
