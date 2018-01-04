using System;
using Moq;
using NUnit.Framework;
using zipkin4net.Internal.Recorder;
using zipkin4net.Tracers.Zipkin;
using zipkin4net.Utils;

namespace zipkin4net.UTest.Internal.Reporter
{
    [TestFixture]
    public class T_MutableSpanMap
    {
        private Mock<IReporter> _reporter = new Mock<IReporter>();

        [Test]
        public void SpansShouldBeFlushedAfterTtl()
        {
            var spanMap = new MutableSpanMap(_reporter.Object);

            var now = TimeUtils.UtcNow;

            var firstSpan = spanMap.GetOrCreate(CreateTraceContext(), (t) => new Span(t, now));

            var timeBeforeTtl = now.AddSeconds(MutableSpanMap.TimeToLive.TotalSeconds - 4);

            //firstSpan.Duration = timeBeforeTTl - now = TTL - 4
            spanMap.FlushOldSpans(timeBeforeTtl);
            _reporter.Verify(r => r.Report(firstSpan), Times.Never(),
                $"Span {nameof(firstSpan)} shouldn't have been flushed as it duration should be lower than TTL. Duration: {firstSpan.Duration}, TTL: {MutableSpanMap.TimeToLive}");

            var newSpanStartTime = timeBeforeTtl;
            var liveSpan = spanMap.GetOrCreate(CreateTraceContext(), (t) => new Span(t, newSpanStartTime));

            var timeAfterTtl = now.AddSeconds(MutableSpanMap.TimeToLive.TotalSeconds + 1);

            //firstSpan.Duration = timeAfterTtl - now = TTL + 1
            //liveSpan.Duration = timeAfterTtl - timeBeforeTtl = 5
            spanMap.FlushOldSpans(timeAfterTtl);
            _reporter.Verify(r => r.Report(firstSpan), Times.Once(),
                $"Span {nameof(firstSpan)} should have been flushed as it duration should be higher than TTL. Duration: {firstSpan.Duration}, TTL: {MutableSpanMap.TimeToLive}");
            _reporter.Verify(r => r.Report(liveSpan), Times.Never(),
                $"Span {nameof(liveSpan)} shouldn't have been flushed as it duration should be lower than TTL. Duration: {liveSpan.Duration}, TTL: {MutableSpanMap.TimeToLive}");
        }
        
        [Test]
        public void StatisticsAreUpdatedForFlush()
        {
            var statistics = new Mock<IStatistics>();
            var spanMap = new MutableSpanMap(_reporter.Object, statistics.Object);

            var spanTimestamp = TimeUtils.UtcNow;
            spanMap.GetOrCreate(CreateTraceContext(), (t) => new Span(t, spanTimestamp));

            var flushTime = spanTimestamp.AddSeconds(MutableSpanMap.TimeToLive.TotalSeconds * 2);
            spanMap.FlushOldSpans(flushTime);

            statistics.Verify(s => s.UpdateSpanFlushed(), Times.Once());
        }

        private static ITraceContext CreateTraceContext()
        {
            return new SpanState(traceId: new Random().Next(), parentSpanId: 0, spanId: 1, isSampled: null,
                isDebug: false);
        }
    }
}