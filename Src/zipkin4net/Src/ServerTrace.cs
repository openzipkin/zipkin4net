using zipkin4net.Annotation;
using System;
using System.Threading.Tasks;

namespace zipkin4net
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

        public virtual void Error(Exception ex)
        {
            Trace.RecordAnnotation(Annotations.Tag("error", ex.Message));
        }
    }

    public static class ServerTraceExtensions
    {
        /// <summary>
        /// Runs the task asynchronously and adds an error annotation in case of failure
        /// </summary>
        /// <param name="serverTrace"></param>
        /// <param name="task"></param>
        public static async Task TracedActionAsync(this ServerTrace serverTrace, Task task)
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