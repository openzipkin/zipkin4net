namespace Criteo.Profiling.Tracing.Transport
{
    public interface ITraceExtractor<in TE>
    {
        bool TryExtract(TE carrier, out Trace trace);
    }
}
