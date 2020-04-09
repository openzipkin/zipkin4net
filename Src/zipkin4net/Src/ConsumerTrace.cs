using System;
using System.Threading.Tasks;
using zipkin4net.Annotation;
using zipkin4net.Propagation;

namespace zipkin4net
{
    public class ConsumerTrace : IDisposable
    {
        public Trace Trace { get; }

        public ConsumerTrace(string serviceName, string rpc, string encodedTraceId, string encodedSpanId,
            string encodedParentSpanId, string sampledStr, string flagsStr)
        {
            var producerTrace = Trace.CreateFromId(
                ExtractorHelper.TryParseTrace(
                    encodedTraceId,
                    encodedSpanId,
                    encodedParentSpanId,
                    sampledStr,
                    flagsStr));
            Trace = producerTrace.Child();
            Trace.Current = Trace;

            Trace.Record(Annotations.ConsumerStart());
            Trace.Record(Annotations.ServiceName(serviceName));
            Trace.Record(Annotations.Rpc(rpc));
        }

        public void AddAnnotation(IAnnotation annotation)
        {
            Trace.Record(annotation);
        }

        public virtual void Error(Exception ex)
        {
            Trace.Record(Annotations.Tag("error", ex.Message));
        }

        public void Dispose()
        {
            Trace.Record(Annotations.ConsumerStop());
        }
    }

    public static class ConsumerTraceExtensions
    {
        /// <summary>
        /// Runs the task asynchronously and adds an error annotation in case of failure
        /// </summary>
        /// <param name="consumerTrace"></param>
        /// <param name="task"></param>
        public static async Task TracedActionAsync(this ConsumerTrace serverTrace, Task task)
        {
            try
            {
                await task;
            }
            catch (Exception ex)
            {
                serverTrace?.Error(ex);
                throw;
            }
        }
    }
}
