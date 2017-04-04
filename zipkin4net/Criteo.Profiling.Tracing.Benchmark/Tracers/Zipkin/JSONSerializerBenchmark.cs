using System;
using System.IO;
using BenchmarkDotNet.Attributes;
using Criteo.Profiling.Tracing.Tracers.Zipkin;

namespace Criteo.Profiling.Tracing.Benchmark.Tracers.Zipkin
{
    public class JSONSerializerBenchmark
    {
        private readonly Span localSpan = CreateLocalSpan();
        private readonly Span clientSpan = CreateClientSpan();
        private readonly Span serverSpan = CreateServerSpan();
        private readonly JSONSpanSerializer jsonSerializer = new JSONSpanSerializer();

        [Benchmark]
        public void writeLocalSpanJSONWithCustomParser()
        {
            writeSpanWithCustomParser(localSpan);
        }

        [Benchmark]
        public void writeClientSpanJSONWithCustomParser()
        {
            writeSpanWithCustomParser(clientSpan);
        }

        [Benchmark]
        public void writeServerSpanJSONWithCustomParser()
        {
            writeSpanWithCustomParser(serverSpan);
        }

        private void writeSpanWithCustomParser(Span span)
        {
            using (var stream = new MemoryStream())
            {
                jsonSerializer.SerializeTo(stream, span);
            }
        }
        

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
}