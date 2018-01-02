using zipkin4net.Tracers.Zipkin;

namespace zipkin4net.Internal.Recorder
{
    internal interface IReporter
    {
        void Report(Span span);
    }
}