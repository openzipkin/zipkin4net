using System;

namespace zipkin4net
{
    public class ProducerTrace : BaseStandardTrace, IDisposable
    {
        public ProducerTrace(string serviceName, string rpc)
        {
            if (Trace.Current != null)
            {
                Trace = Trace.Current.Child();
            }

            Trace.Record(Annotations.ProducerStart());
            Trace.Record(Annotations.ServiceName(serviceName));
            Trace.Record(Annotations.Rpc(rpc));
        }

        public void Dispose()
        {
            Trace.Record(Annotations.ProducerStop());
        }
    }
}
