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
        private static readonly ICollection<ITracer> tracers = new List<ITracer>();

        internal static ICollection<ITracer> Tracers
        {
            get { return tracers; }
        }

        /// <summary>
        /// Adds a new tracer. Subsequent trace records will be forwarded to the tracer.
        /// </summary>
        /// <param name="tracer"></param>
        public static void Register(ITracer tracer)
        {
            Tracers.Add(tracer);
        }

        /// <summary>
        /// Clears the registered tracers. This is not thread-safe.
        /// Should be used for debugging and testing purposes.
        /// </summary>
        internal static void Clear()
        {
            tracers.Clear();
        }

    }
}
