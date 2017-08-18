namespace zipkin4net.Tracers
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
