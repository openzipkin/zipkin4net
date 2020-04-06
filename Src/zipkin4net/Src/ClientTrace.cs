using System;
using System.Threading.Tasks;

namespace zipkin4net
{
    public class ClientTrace : BaseStandardTrace, IDisposable
    {
        public ClientTrace(string serviceName, string rpc)
        {
            if (Trace.Current != null) {
              Trace = Trace.Current.Child();
            }

            Trace.Record(Annotations.ClientSend());
            Trace.Record(Annotations.ServiceName(serviceName));
            Trace.Record(Annotations.Rpc(rpc));
        }

        public void Dispose()
        {
            Trace.Record(Annotations.ClientRecv());
        }
    }

    public static class ClientTraceExtensions
    {
        /// <summary>
        /// Runs the task asynchronously and adds an error annotation in case of failure
        /// </summary>
        /// <param name="clientTrace"></param>
        /// <param name="task"></param>
        public static async Task<T> TracedActionAsync<T>(this ClientTrace clientTrace, Task<T> task)
        {
            try
            {
                return await task;
            }
            catch (Exception ex)
            {
                clientTrace?.Error(ex);
                throw;
            }
        }
    }
}
