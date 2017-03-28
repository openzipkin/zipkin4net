using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using Criteo.Profiling.Tracing.Utils;

namespace Criteo.Profiling.Tracing.Tracers.Zipkin
{
    /// <summary>
    /// Tracer which logs annotations in Twitter Zipkin format.
    ///
    /// Inspired by Finagle source code:
    /// https://github.com/twitter/finagle/tree/develop/finagle-zipkin
    /// </summary>
    public class ZipkinTracer : ITracer
    {
        public IStatistics Statistics { get; private set; }

        private readonly ConcurrentDictionary<SpanState, Span> _spanMap = new ConcurrentDictionary<SpanState, Span>();
        private readonly IZipkinSender _spanSender;
        private readonly ISpanSerializer _spanSerializer;

        /// <summary>
        /// Flush old records when fired.
        /// </summary>
        private readonly Timer _flushTimer;

        /// <summary>
        /// Spans which are not completed by this time are automatically flushed.
        /// </summary>
        internal static readonly TimeSpan TimeToLive = TimeSpan.FromMinutes(1);

        [Obsolete("Please use ZipkinTracer(IZipkinSender sender, ISpanSerializer spanSerializer, IStatistics statistics)")]
        public ZipkinTracer(IZipkinSender sender, IStatistics statistics = null) : this(sender, new ThriftSpanSerializer(), statistics)
        {
        }

        public ZipkinTracer(IZipkinSender sender, ISpanSerializer spanSerializer, IStatistics statistics = null)
        {
            if (sender == null) throw new ArgumentNullException("sender", "You have to specify a non-null sender for Zipkin tracer.");
            Statistics = statistics ?? new Statistics();
            _spanSender = sender;

            if (sender == null) throw new ArgumentNullException("spanSerializer", "You have to specify a non-null span serializer for Zipkin tracer.");
            _spanSerializer = spanSerializer;

            _flushTimer = new Timer(_ => FlushOldSpans(TimeUtils.UtcNow), null, TimeToLive, TimeToLive);
        }

        public void Record(Record record)
        {
            Statistics.UpdateRecordProcessed();

            var updatedSpan = _spanMap.AddOrUpdate(record.SpanState,
                id => VisitAnnotation(record, new Span(record.SpanState, record.Timestamp)),
                (id, span) => VisitAnnotation(record, span));

            if (updatedSpan.Complete)
            {
                RemoveThenLogSpan(record.SpanState);
            }
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
            {
                LogSpan(spanToLog);
            }
        }

        private void LogSpan(Span span)
        {
            var memoryStream = new MemoryStream();
            _spanSerializer.SerializeTo(memoryStream, span);
            var serializedSpan = memoryStream.ToArray();

            _spanSender.Send(serializedSpan);
            Statistics.UpdateSpanSent();
            Statistics.UpdateSpanSentBytes(serializedSpan.Length);
        }

        /// <summary>
        /// Flush old spans which didn't complete before the end of their TTL.
        /// Visibility is set to internal to allow unit testing.
        /// </summary>
        /// <param name="utcNow"></param>
        internal void FlushOldSpans(DateTime utcNow)
        {
            var outlivedSpans = _spanMap.Where(pair => (utcNow - pair.Value.SpanCreated) > TimeToLive).ToList();

            foreach (var oldSpanEntry in outlivedSpans)
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
