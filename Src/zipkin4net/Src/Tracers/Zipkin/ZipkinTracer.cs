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
        private readonly Recorder _recorder;

        [Obsolete]
        public IStatistics Statistics { get; private set; }

        private readonly MutableSpanMap _spanMap;

        public ZipkinTracer(IZipkinSender sender, ISpanSerializer spanSerializer)
            : this(sender, spanSerializer, new Statistics())
        {
        }

        public ZipkinTracer(IZipkinSender sender, ISpanSerializer spanSerializer, IStatistics statistics)
            : this(new ZipkinTracerReporter(sender, spanSerializer, statistics), statistics)
        {
        }

        internal ZipkinTracer(IReporter reporter, IStatistics statistics)
            : this(reporter, new Recorder(new EndPoint(SerializerUtils.DefaultServiceName, SerializerUtils.DefaultEndPoint), reporter), statistics)
        {
        }
        
        internal ZipkinTracer(IReporter reporter, Recorder recorder, IStatistics statistics)
        {
            if (statistics == null)
            {
                throw new ArgumentNullException(nameof(statistics),
                    "You have to specify a non-null statistics.");
            }
            _recorder = recorder;
            Statistics = statistics;
            _spanMap = new MutableSpanMap(reporter, Statistics);
        }

        [Obsolete("Please use the new IRecorder API")]
        public void Record(Record record)
        {
            Statistics.UpdateRecordProcessed();

            var traceContext = record.SpanState;
            var span = _spanMap.GetOrCreate(traceContext, (t) => new Span(t, record.Timestamp));
            VisitAnnotation(_recorder, record, span);

            if (span.Complete)
            {
                _spanMap.RemoveThenReportSpan(record.SpanState);
            }
        }

        private static void VisitAnnotation(Recorder recorder, Record record, Span span)
        {
            var visitor = new ZipkinAnnotationVisitor(recorder, record, span.SpanState);

            record.Annotation.Accept(visitor);
        }
    }
}