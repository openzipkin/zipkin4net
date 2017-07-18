using System.Collections.Concurrent;

namespace Criteo.Profiling.Tracing.Tracers
{

    /// <summary>
    /// Simply keeps records in memory.
    /// </summary>
    public class InMemoryTracer : ITracer
    {
        public ConcurrentQueue<Record> Records { get; private set; }

        public InMemoryTracer()
        {
            Records = new ConcurrentQueue<Record>();
        }

        public void Record(Record record)
        {
            Records.Enqueue(record);
        }
    }
}
