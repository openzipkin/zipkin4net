using System.IO;
using zipkin4net.Tracers.Zipkin;
using BenchmarkDotNet.Attributes;

namespace zipkin4net.Benchmark.Tracers.Zipkin
{
    public class ThriftSerializerBenchmark
    {
        private readonly ThriftSpanSerializer serializer = new ThriftSpanSerializer();

        [Benchmark]
        public void writeLocalSpanThrift()
        {
            writeSpanWithThrift(Spans.localSpan);
        }

        [Benchmark]
        public void writeClientSpanThrift()
        {
            writeSpanWithThrift(Spans.clientSpan);
        }

        [Benchmark]
        public void writeServerSpanThrift()
        {
            writeSpanWithThrift(Spans.serverSpan);
        }

        private void writeSpanWithThrift(Span span)
        {
            using (var memoryStream = new MemoryStream())
            {
                serializer.SerializeTo(memoryStream, span);
            }
        }
    }
}