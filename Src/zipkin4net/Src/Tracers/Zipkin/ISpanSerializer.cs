using System.IO;

namespace zipkin4net.Tracers.Zipkin
{
    public interface ISpanSerializer
    {

        void SerializeTo(Stream stream, Span span);

    }
}
