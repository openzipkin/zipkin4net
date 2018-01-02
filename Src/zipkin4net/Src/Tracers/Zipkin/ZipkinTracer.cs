using System;
using zipkin4net.Internal.Recorder;

namespace zipkin4net.Tracers.Zipkin
{
    /// <summary>
    /// Tracer which logs annotations in Twitter Zipkin format.
    ///
    /// Inspired by Finagle source code:
    /// https://github.com/twitter/finagle/tree/develop/finagle-zipkin
    /// </summary>
    public class ZipkinTracer : ITracer
    {
        [Obsolete]
        public IStatistics Statistics { get; private set; }

        private readonly MutableSpanMap _spanMap;

        private readonly IReporter _reporter;


        public ZipkinTracer(IZipkinSender sender, ISpanSerializer spanSerializer, IStatistics statistics = null)
            : this(new ZipkinTracerReporter(sender, spanSerializer, statistics), statistics)
        {
        }

        internal ZipkinTracer(IReporter reporter, IStatistics statistics)
        {
            Statistics = statistics ?? new Statistics();
            _reporter = reporter;
            _spanMap = new MutableSpanMap(_reporter, Statistics);
        }

        public void Record(Record record)
        {
            Statistics.UpdateRecordProcessed();

            var traceContext = record.SpanState;
            var span = _spanMap.GetOrCreate(traceContext, (t) => new Span(t, record.Timestamp));
            VisitAnnotation(record, span);

            if (span.Complete)
            {
                RemoveThenLogSpan(record.SpanState);
            }
        }

        private static void VisitAnnotation(Record record, Span span)
        {
            var visitor = new ZipkinAnnotationVisitor(record, span);

            record.Annotation.Accept(visitor);
        }

        private void RemoveThenLogSpan(ITraceContext spanState)
        {
            var spanToLog = _spanMap.Remove(spanState);
            if (spanToLog != null)
            {
                _reporter.Report(spanToLog);
            }
        }
    }
}