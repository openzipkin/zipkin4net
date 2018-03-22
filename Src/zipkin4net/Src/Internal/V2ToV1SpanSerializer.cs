using System;
using System.IO;
using zipkin4net.Tracers.Zipkin;

namespace zipkin4net.Internal
{
    internal class V2ToV1SpanSerializer : ISpanSerializer<V2.Span>
    {
        private readonly ISpanSerializer<Span> _spanSerializer;

        public V2ToV1SpanSerializer(ISpanSerializer<Span> spanSerializer)
        {
            if (spanSerializer == null)
            {
                throw new ArgumentNullException(nameof(spanSerializer),
                    "You have to specify a non-null span serializer.");
            }
            _spanSerializer = spanSerializer;
        }
        
        public void SerializeTo(Stream stream, V2.Span span)
        {
            _spanSerializer.SerializeTo(stream, V2SpanConverter.ToSpan(span));
        }
    }
}