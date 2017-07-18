using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Criteo.Profiling.Tracing.Batcher;
using Criteo.Profiling.Tracing.Utils;

namespace Criteo.Profiling.Tracing.Tracers.Zipkin
{
    /// <summary>
    ///  Tracer which logs annotations in Twitter Zipkin format.
    ///  Inspired by Finagle source code:
    ///  https://github.com/twitter/finagle/tree/develop/finagle-zipkin
    /// </summary>
    public class ZipkinTracer : ITracer
    {
        /// <summary>
        /// Spans which are not completed by this time are automatically flushed.
        /// </summary>
        internal static readonly TimeSpan TimeToLive = TimeSpan.FromMinutes(1);

        /// <summary>
        /// Flush old records when fired.
        /// </summary>
        private readonly Timer _flushTimer;

        private readonly ConcurrentDictionary<SpanState, Span> _spanMap = new ConcurrentDictionary<SpanState, Span>();
        private readonly ISpanProcessor _spanProcessor;


        [Obsolete(
            "Please use ZipkinTracer(IZipkinSender sender, ISpanSerializer spanSerializer, IStatistics statistics)")]
        public ZipkinTracer(IZipkinSender sender, IStatistics statistics = null)
            : this(sender, new ThriftSpanSerializer(), statistics)
        {
        }

        public ZipkinTracer(IZipkinSender sender, ISpanSerializer spanSerializer, IStatistics statistics = null)
        {
            Statistics = statistics ?? new Statistics();
            _spanProcessor = new ZipkinSimpleSpanProcessor(sender, spanSerializer, Statistics);
        }

        public ZipkinTracer(ISpanProcessor spanProcessor, IStatistics statistics)
        {
            _spanProcessor = Guard.IsNotNull(spanProcessor, nameof(spanProcessor));

            Statistics = Guard.IsNotNull(statistics, nameof(statistics));
            _flushTimer = new Timer(_ => FlushOldSpans(TimeUtils.UtcNow), null, TimeToLive, TimeToLive);
        }

        public IStatistics Statistics { get; }


        public void Record(Record record)
        {
            Statistics.UpdateRecordProcessed();

            var updatedSpan = _spanMap.AddOrUpdate(record.SpanState,
                id => VisitAnnotation(record, new Span(record.SpanState, record.Timestamp)),
                (id, span) => VisitAnnotation(record, span));

            if (updatedSpan.Complete)
                RemoveThenLogSpan(record.SpanState);
        }

        private static Span VisitAnnotation(Record record, Span span)
        {
            var visitor = new ZipkinAnnotationVisitor(record, span);

            record.Annotation.Accept(visitor);

            return span;
        }

        private void RemoveThenLogSpan(SpanState spanState)
        {
            Span spanToLog;
            if (_spanMap.TryRemove(spanState, out spanToLog))
                _spanProcessor.LogSpan(spanToLog);
        }

        /// <summary>
        /// Flush old spans which didn't complete before the end of their TTL.
        /// Visibility is set to internal to allow unit testing.
        /// </summary>
        /// <param name="utcNow"></param>
        internal void FlushOldSpans(DateTime utcNow)
        {
            List<KeyValuePair<SpanState, Span>> outlivedSpans =
                _spanMap.Where(pair => utcNow - pair.Value.SpanCreated > TimeToLive).ToList();

            foreach (KeyValuePair<SpanState, Span> oldSpanEntry in outlivedSpans)
            {
                if (!oldSpanEntry.Value.Complete)
                {
                    oldSpanEntry.Value.AddAnnotation(new ZipkinAnnotation(TimeUtils.UtcNow, "flush.timeout"));
                    Statistics.UpdateSpanFlushed();
                }
                RemoveThenLogSpan(oldSpanEntry.Key);
            }
        }
    }
}