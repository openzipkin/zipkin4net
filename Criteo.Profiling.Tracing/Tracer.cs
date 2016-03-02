using System;
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
        private static ICollection<ITracer> _tracers = new List<ITracer>();

        internal static ICollection<ITracer> Tracers
        {
            get { return _tracers; }
        }

        /// <summary>
        /// Adds a new tracer. Subsequent annotation records will be forwarded to the tracer.
        /// </summary>
        /// <param name="tracer"></param>
        public static void Register(ITracer tracer)
        {
            _tracers.Add(tracer);
        }

        /// <summary>
        /// Clears the registered tracers.
        /// </summary>
        public static void Clear()
        {
            _tracers = new List<ITracer>();
        }

        /// <summary>
        /// Send a record to all the registered tracers
        /// </summary>
        /// <param name="record"></param>
        internal static void Push(Record record)
        {
            foreach (var tracer in _tracers)
            {
                try
                {
                    tracer.Record(record);
                }
                catch (Exception ex)
                {
                    // No exception coming for traces should disrupt the main application as tracing is optional.
                    Trace.Configuration.Logger.LogWarning("An error occured while recording the annotation. Msg: " + ex.Message);
                }
            }
        }

    }
}
