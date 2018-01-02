using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using zipkin4net.Tracers.Zipkin;
using zipkin4net.Utils;

namespace zipkin4net.Internal.Recorder
{
    internal class MutableSpanMap
    {
        private readonly IReporter _reporter;
        private readonly IStatistics _statistics;

        private readonly ConcurrentDictionary<ITraceContext, Span> _spanMap =
            new ConcurrentDictionary<ITraceContext, Span>();

        /// <summary>
        /// Flush old records when fired.
        /// </summary>
        private readonly Timer _flushTimer;

        /// <summary>
        /// Spans which are not completed by this time are automatically flushed.
        /// </summary>
        internal static readonly TimeSpan TimeToLive = TimeSpan.FromMinutes(1);

        internal MutableSpanMap(IReporter reporter)
            : this(reporter, new Statistics())
        {
        }

        internal MutableSpanMap(IReporter reporter, IStatistics statistics)
        {
            _reporter = reporter;
            _statistics = statistics;
            _flushTimer = new Timer(_ => FlushOldSpans(TimeUtils.UtcNow), null, TimeToLive, TimeToLive);
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
                    _statistics.UpdateSpanFlushed();
                }

                RemoveThenReportSpan(oldSpanEntry.Key);
            }
        }

        public Span GetOrCreate(ITraceContext traceContext, Func<ITraceContext, Span> spanCreator)
        {
            return _spanMap.GetOrAdd(traceContext, spanCreator);
        }

        public Span Get(ITraceContext context)
        {
            Span span;
            if (!_spanMap.TryGetValue(context, out span))
            {
                throw new InvalidOperationException($"Span associated with {context} couldn't be found");
            }

            return span;
        }

        public Span Remove(ITraceContext traceContext)
        {
            Span span = null;
            _spanMap.TryRemove(traceContext, out span);
            return span; //Will return null if span doesn't exist
        }
        
        public void RemoveThenReportSpan(ITraceContext spanState)
        {
            var spanToLog = Remove(spanState);
            if (spanToLog != null)
            {
                _reporter.Report(spanToLog);
            }
        }
    }
}