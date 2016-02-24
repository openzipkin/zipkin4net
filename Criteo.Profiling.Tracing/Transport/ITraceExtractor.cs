namespace Criteo.Profiling.Tracing.Transport
{
    public interface ITraceExtractor<in TE>
    {
        bool TryExtract(TE transport, out Trace trace);
    }
}
