using Criteo.Profiling.Tracing.Tracers.Zipkin;

namespace Criteo.Profiling.Tracing.Batcher
{
    public interface ISpanProcessor
    {
        void LogSpan(Span spanToLog);
    }
}