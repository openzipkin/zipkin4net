namespace Criteo.Profiling.Tracing.Transport
{
    public interface ITraceInjector<in TE>
    {
        bool Inject(Trace trace, TE transport);
    }
}
