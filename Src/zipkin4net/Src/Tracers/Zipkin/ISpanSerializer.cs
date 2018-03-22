using System.IO;

namespace zipkin4net.Tracers.Zipkin
{
    public interface ISpanSerializer<S>
    {
        void SerializeTo(Stream stream, S span);
    }
    
    public interface ISpanSerializer : ISpanSerializer<Span>
    {
    }
}
