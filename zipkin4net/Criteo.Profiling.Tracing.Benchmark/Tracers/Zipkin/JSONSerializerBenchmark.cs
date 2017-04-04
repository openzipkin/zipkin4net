using System;
using System.IO;
using BenchmarkDotNet.Attributes;
using Criteo.Profiling.Tracing.Tracers.Zipkin;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Criteo.Profiling.Tracing.Benchmark.Tracers.Zipkin
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

    public static class Spans
    {
        public static Span localSpan = CreateLocalSpan();
        public static Span clientSpan = CreateClientSpan();
        public static Span serverSpan = CreateServerSpan();

        private static Span CreateLocalSpan()
        {
            var trace = Trace.Create();
            trace.Record(Annotations.LocalOperationStart("JDBCSpanStore"));
            trace.Record(Annotations.ServiceName("get-traces"));
            trace.Record(Annotations.Tag("request", "QueryRequest{serviceName=zipkin-server, spanName=null, annotations=[], binaryAnnotations={}, minDuration=null, maxDuration=null, endTs=1461750033209, lookback=604800000, limit=10}"));
            trace.Record(Annotations.LocalOperationStop());
            return new Span(trace.CurrentSpan, new DateTime());
        }

        private static Span CreateClientSpan()
        {
            var trace = Trace.Create();
            trace.Record(Annotations.ClientSend());
            trace.Record(Annotations.Rpc("query"));
            trace.Record(Annotations.ServiceName("zipkin-client"));
            trace.Record(Annotations.Tag("jdbc.query", "select distinct `zipkin_spans`.`trace_id` from `zipkin_spans` join `zipkin_annotations` on (`zipkin_spans`.`trace_id` = `zipkin_annotations`.`trace_id` and `zipkin_spans`.`id` = `zipkin_annotations`.`span_id`) where (`zipkin_annotations`.`endpoint_service_name` = ? and `zipkin_spans`.`start_ts` between ? and ?) order by `zipkin_spans`.`start_ts` desc limit ?"));
            trace.Record(Annotations.ClientRecv());
            return new Span(trace.CurrentSpan, new DateTime());
        }

        private static Span CreateServerSpan()
        {
            var trace = Trace.Create();
            trace.Record(Annotations.ServerRecv());
            trace.Record(Annotations.Rpc("post"));
            trace.Record(Annotations.ServiceName("zipkin-server"));
            trace.Record(Annotations.Tag("srv/finagle.version", "6.34.0"));
            trace.Record(Annotations.ServerSend());
            return new Span(trace.CurrentSpan, new DateTime());
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