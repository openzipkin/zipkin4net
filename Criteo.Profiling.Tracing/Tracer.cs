using System.Collections.Generic;

namespace Criteo.Profiling.Tracing
{
    /// <summary>
    /// Keeps track of all the registered tracers.
    /// </summary>
    public static class Tracer
    {
        /// <summary>
        /// Global list of registred tracers.
        /// </summary>
        private static ICollection<ITracer> tracers = new List<ITracer>();

        internal static ICollection<ITracer> Tracers
        {
            get { return tracers; }
        }

        /// <summary>
        /// Adds a new tracer. Subsequent annotation records will be forwarded to the tracer.
        /// </summary>
        /// <param name="tracer"></param>
        public static void Register(ITracer tracer)
        {
            tracers.Add(tracer);
        }

        /// <summary>
        /// Clears the registered tracers.
        /// </summary>
        public static void Clear()
        {
            tracers = new List<ITracer>();
        }

    }
}
