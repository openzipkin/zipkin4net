using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Criteo.Profiling.Tracing.Utils
{
    public static class TraceHelper
    {
        /// <summary>
        /// Runs the action and adds an error annotation in case of failure
        /// </summary>
        /// <param name="action"></param>
        public static void TracedAction(Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                Trace.Current?.Record(Annotations.Tag("error", ex.Message));

                throw;
            }
        }

        /// <summary>
        /// Runs the task asynchronously and adds an error annotation in case of failure
        /// </summary>
        /// <param name="task"></param>
        public static async Task TracedActionAsync(Task task)
        {
            try
            {
                await task;
            }
            catch (Exception ex)
            {
                Trace.Current?.Record(Annotations.Tag("error", ex.Message));

                throw;
            }
        }

        /// <summary>
        /// Runs the task asynchronously and adds an error annotation in case of failure
        /// </summary>
        /// <param name="task"></param>
        public static async Task<T> TracedActionAsync<T>(Task<T> task)
        {
            try
            {
                return await task;
            }
            catch (Exception ex)
            {
                Trace.Current?.Record(Annotations.Tag("error", ex.Message));

                throw;
            }
        }
    }
}
