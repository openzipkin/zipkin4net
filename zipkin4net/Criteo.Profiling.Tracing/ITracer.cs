namespace Criteo.Profiling.Tracing
{
    public interface ITracer
    {
        void Record(Record record);
    }
}
