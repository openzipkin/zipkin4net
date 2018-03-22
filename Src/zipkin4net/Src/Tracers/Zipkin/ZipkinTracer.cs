using System;
using zipkin4net.Internal;
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
        [Obsolete] public IStatistics Statistics { get; private set; }

        private readonly MutableSpanMap _spanMap;

        private readonly IReporter<Internal.V2.Span> _reporter;
        private readonly Endpoint _localEndpoint;

        private static readonly Endpoint DefaultLocalEndpoint =
            new Endpoint(SerializerUtils.DefaultServiceName, SerializerUtils.DefaultEndPoint);


        public ZipkinTracer(IZipkinSender sender, ISpanSerializer spanSerializer)
            : this(sender, spanSerializer, new Statistics())
        {
        }

        public ZipkinTracer(IZipkinSender sender, ISpanSerializer spanSerializer, IStatistics statistics)
            : this(new ZipkinTracerReporter<Internal.V2.Span>(sender, new V2ToV1SpanSerializer(spanSerializer), statistics), DefaultLocalEndpoint, statistics)
        {
        }

        internal ZipkinTracer(IReporter<Internal.V2.Span> reporter, Endpoint localEndpoint, IStatistics statistics)
        {
            if (statistics == null)
            {
                throw new ArgumentNullException(nameof(statistics),
                    "You have to specify a non-null statistics.");
            }

            Statistics = statistics;
            _reporter = reporter;
            _localEndpoint = localEndpoint;
            _spanMap = new MutableSpanMap(_reporter, Statistics);
        }

        public void Record(Record record)
        {
            Statistics.UpdateRecordProcessed();

            var traceContext = record.SpanState;
            if (!(traceContext.Sampled ?? false)) return;
            var span = _spanMap.GetOrCreate(traceContext, (t) =>
            {
                var mutableSpan = new MutableSpan(traceContext, _localEndpoint);
                mutableSpan.Start(record.Timestamp);
                return mutableSpan;
            });
            VisitAnnotation(record, span);

            if (span.Finished)
            {
                RemoveThenLogSpan(record.SpanState);
            }
        }

        private static void VisitAnnotation(Record record, MutableSpan span)
        {
            var visitor = new ZipkinAnnotationVisitor(record, span);

            record.Annotation.Accept(visitor);
        }

        private void RemoveThenLogSpan(ITraceContext spanState)
        {
            var spanToLog = _spanMap.Remove(spanState);
            if (spanToLog != null)
            {
                _reporter.Report(spanToLog.ToSpan());
            }
        }
    }
}