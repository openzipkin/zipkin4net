namespace Criteo.Profiling.Tracing.Tracers
{
    /// <summary>
    /// Empty tracer which simply discards records.
    /// </summary>
    public class VoidTracer : ITracer
    {
        public void Record(Record record)
        {
        }

    }
}
