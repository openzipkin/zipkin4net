using System.IO;
using BenchmarkDotNet.Attributes;
using zipkin4net.Tracers.Zipkin;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace zipkin4net.Benchmark.Tracers.Zipkin
{
    public class JSONSerializerBenchmark
    {
        private readonly JSONSpanSerializer jsonSerializer = new JSONSpanSerializer();

        [Benchmark]
        public void writeLocalSpanJSONWithCustomParser()
        {
            writeSpanWithCustomParser(Spans.localSpan);
        }

        [Benchmark]
        public void writeClientSpanJSONWithCustomParser()
        {
            writeSpanWithCustomParser(Spans.clientSpan);
        }

        [Benchmark]
        public void writeServerSpanJSONWithCustomParser()
        {
            writeSpanWithCustomParser(Spans.serverSpan);
        }

        [Benchmark]
        public void writeLocalSpanJSONWithJSONDotNetParserWithoutList()
        {
            writeSpanWithJSONDotNetParser(jsondotnetSpans.localSpan);
        }

        [Benchmark]
        public void writeClientSpanJSONWithJSONDotNetParserWithoutList()
        {
            writeSpanWithJSONDotNetParser(jsondotnetSpans.clientSpan);
        }

        [Benchmark]
        public void writeServerSpanJSONWithJSONDotNetParserWithoutList()
        {
            writeSpanWithJSONDotNetParser(jsondotnetSpans.serverSpan);
        }

    [Benchmark]
        public void writeLocalSpanJSONWithJSONDotNetParserWithList()
        {
            writeSpanWithJSONDotNetParser(jsondotnetSpans.listOfLocalSpan);
        }

        [Benchmark]
        public void writeClientSpanJSONWithJSONDotNetParserWithList()
        {
            writeSpanWithJSONDotNetParser(jsondotnetSpans.listOfClientSpan);
        }

        [Benchmark]
        public void writeServerSpanJSONWithJSONDotNetParserWithList()
        {
            writeSpanWithJSONDotNetParser(jsondotnetSpans.listOfserverSpan);
        }

        [Benchmark]
        public void writeLocalSpanJSONWithJSONDotNetParserWithListCreation()
        {
            writeSpanWithJSONDotNetParser(new List<jsondotnet.Span>() { jsondotnetSpans.localSpan });
        }

        [Benchmark]
        public void writeClientSpanJSONWithJSONDotNetParserWithListCreation()
        {
            writeSpanWithJSONDotNetParser(new List<jsondotnet.Span>() { jsondotnetSpans.clientSpan });
        }

        [Benchmark]
        public void writeServerSpanJSONWithJSONDotNetParserWithListCreation()
        {
            writeSpanWithJSONDotNetParser(new List<jsondotnet.Span>() { jsondotnetSpans.serverSpan });
        }

        private void writeSpanWithCustomParser(Span span)
        {
            using (var stream = new MemoryStream())
            {
                jsonSerializer.SerializeTo(stream, span);
            }
        }

        private void writeSpanWithJSONDotNetParser(object value)
        {
            JsonConvert.SerializeObject(value);
        }
    }

    public static class jsondotnetSpans
    {
        public static jsondotnet.Span localSpan = new jsondotnet.Span(Spans.localSpan);
        public static jsondotnet.Span clientSpan = new jsondotnet.Span(Spans.clientSpan);
        public static jsondotnet.Span serverSpan = new jsondotnet.Span(Spans.serverSpan);

        public static List<jsondotnet.Span> listOfLocalSpan = new List<jsondotnet.Span>() { localSpan };
        public static List<jsondotnet.Span> listOfClientSpan = new List<jsondotnet.Span>() { clientSpan };
        public static List<jsondotnet.Span> listOfserverSpan = new List<jsondotnet.Span>() { serverSpan };
    }
}