using System;
using zipkin4net.Tracers.Zipkin;

namespace zipkin4net.Benchmark.Tracers.Zipkin
{
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
            trace.Record(Annotations.Tag("ca", "frontend"));
            trace.Record(Annotations.Tag("sa", "backend"));
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
            trace.Record(Annotations.Tag("http.path", "/api"));
            trace.Record(Annotations.ServerSend());
            return new Span(trace.CurrentSpan, new DateTime());
        }
    }
}