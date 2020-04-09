using System;
using zipkin4net.Propagation;

namespace zipkin4net
{
    public class ConsumerTrace : BaseStandardTrace, IDisposable
    {
        public ConsumerTrace(string serviceName, string rpc, string encodedTraceId, string encodedSpanId,
            string encodedParentSpanId, string sampledStr, string flagsStr)
        {
            var spanState = ExtractorHelper.TryParseTrace(encodedTraceId, encodedSpanId, encodedParentSpanId,
                sampledStr, flagsStr);

            if (spanState != default(SpanState))
            {
                Trace = Trace.CreateFromId(spanState).Child();
            }
            else
            {
                Trace = Trace.Create();
            }

            Trace.Current = Trace;

            Trace.Record(Annotations.ConsumerStart());
            Trace.Record(Annotations.ServiceName(serviceName));
            Trace.Record(Annotations.Rpc(rpc));
        }

        public void Dispose()
        {
            Trace.Record(Annotations.ConsumerStop());
        }
    }
}
