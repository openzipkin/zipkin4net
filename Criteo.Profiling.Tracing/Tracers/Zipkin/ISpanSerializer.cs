using System.IO;

namespace Criteo.Profiling.Tracing.Tracers.Zipkin
{
    internal interface ISpanSerializer
    {

        void SerializeTo(Stream stream, Span span);

    }
}
