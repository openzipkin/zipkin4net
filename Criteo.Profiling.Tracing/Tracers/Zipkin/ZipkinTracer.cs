using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using Thrift.Protocol;
using Thrift.Transport;

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
        private readonly ConcurrentDictionary<SpanId, Span> spanMap = new ConcurrentDictionary<SpanId, Span>();
        private readonly IZipkinSender spanSender;

        /// <summary>
        /// Flush old records when fired.
        /// </summary>
        private readonly Timer flushTimer;

        /// <summary>
        /// Time-to-live expressed in seconds.
        /// Spans which are not completed by this time are automatically flushed.
        /// </summary>
        internal const int TimeToLive = 10;

        public ZipkinTracer(IZipkinSender sender)
        {
            if (sender == null) throw new ArgumentNullException("sender", "You have to specify a non-null sender for Zipkin tracer.");
            this.spanSender = sender;
            this.flushTimer = new Timer(_ => FlushOldSpans(DateTime.UtcNow), null, TimeSpan.FromSeconds(TimeToLive), TimeSpan.FromSeconds(TimeToLive));
        }

        public void Record(Record record)
        {
            var updatedSpan = spanMap.AddOrUpdate(record.SpanId,
                id => VisitAnnotation(record, new Span(record.SpanId, record.Timestamp)),
                (id, span) => VisitAnnotation(record, span));

            if (updatedSpan.Complete)
            {
                RemoveThenLogSpan(record.SpanId);
            }
        }

        private static Span VisitAnnotation(Record record, Span span)
        {
            var visitor = new ZipkinAnnotationVisitor(record, span);

            record.Annotation.Accept(visitor);

            return span;
        }

        private void RemoveThenLogSpan(SpanId spanId)
        {
            Span spanToLog;
            if (spanMap.TryRemove(spanId, out spanToLog))
            {
                LogSpan(spanToLog);
            }
        }

        private void LogSpan(Span span)
        {
            var serializedSpan = ThriftSerialize(span);
            spanSender.Send(serializedSpan);
        }

        private static byte[] ThriftSerialize(Span span)
        {
            var thriftSpan = span.ToThrift();

            var transport = new TMemoryBuffer();
            var protocol = new TBinaryProtocol(transport);

            thriftSpan.Write(protocol);

            return transport.GetBuffer();
        }

        /// <summary>
        /// Flush old spans which didn't complete before the end of their TTL.
        /// Visibility is set to internal to allow unit testing.
        /// </summary>
        /// <param name="utcNow"></param>
        internal void FlushOldSpans(DateTime utcNow)
        {
            spanMap.Where(pair => utcNow.Subtract(pair.Value.Started).TotalSeconds > TimeToLive).AsParallel().ForAll(pair =>
            {
                if (!pair.Value.Complete) pair.Value.AddAnnotation(new ZipkinAnnotation(DateTime.UtcNow, "flush.timeout"));
                RemoveThenLogSpan(pair.Key);
            });
        }

    }
}
