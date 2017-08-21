using System.Collections.Concurrent;

namespace zipkin4net.Tracers
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
