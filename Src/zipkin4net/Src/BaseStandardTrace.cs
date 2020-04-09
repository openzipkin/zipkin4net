using zipkin4net.Annotation;
using System;
using System.Threading.Tasks;

namespace zipkin4net
{
    public class BaseStandardTrace
    {
        public virtual Trace Trace { internal set; get; }

        public void AddAnnotation(IAnnotation annotation)
        {
            Trace.Record(annotation);
        }

        public virtual void Error(Exception ex)
        {
            Trace.Record(Annotations.Tag("error", ex.Message));
        }
    }

    public static class BaseStandardTraceExtensions
    {
        /// <summary>
        /// Runs the task asynchronously with custom return type and
        /// adds an error annotation in case of failure
        /// </summary>
        /// <param name="trace"></param>
        /// <param name="task"></param>
        public static async Task<T> TracedActionAsync<T>(this BaseStandardTrace trace, Task<T> task)
        {
            try
            {
                return await task;
            }
            catch (Exception ex)
            {
                trace?.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Runs the task asynchronously and
        /// adds an error annotation in case of failure
        /// </summary>
        /// <param name="trace"></param>
        /// <param name="task"></param>
        public static async Task TracedActionAsync(this BaseStandardTrace trace, Task task)
        {
            try
            {
                await task;
            }
            catch (Exception ex)
            {
                trace?.Error(ex);
                throw;
            }
        }
    }
}
