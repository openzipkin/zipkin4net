﻿using System;
using System.IO;
using Criteo.Profiling.Tracing.Annotation;
using Criteo.Profiling.Tracing.Batcher;
using Criteo.Profiling.Tracing.Tracers.Zipkin;
using Criteo.Profiling.Tracing.Utils;
using Moq;
using NUnit.Framework;

namespace Criteo.Profiling.Tracing.UTest.Tracers.Zipkin
{
    [TestFixture]
    internal class T_ZipkinTracer
    {
        private Mock<ISpanProcessor> _spanBatcher;
        private Mock<IStatistics> _statistics;
        private ZipkinTracer _tracer;

        [SetUp]
        public void Setup()
        {
            _spanBatcher = new Mock<ISpanProcessor>();
            _statistics = new Mock<IStatistics>();

            _tracer = new ZipkinTracer(_spanBatcher.Object, _statistics.Object);
        }

        [Test]
        public void ShouldThrowWithNullSender()
        {
            IZipkinSender sender = null;
            Assert.Throws<ArgumentNullException>(() => { var tracer = new ZipkinTracer(sender, Mock.Of<ISpanSerializer>());});
        }

        [Test]
        public void ShouldThrowWithNullSerializer()
        {
            ISpanSerializer spanSerializer = null;
            Assert.Throws<ArgumentNullException>(() => { var tracer = new ZipkinTracer(Mock.Of<IZipkinSender>(), spanSerializer);});
        }

        [Test]
        public void SpansAreLoggedAfterEndAnnotationClientRecv()
        {
            var trace = Trace.Create();

            Record(trace, Annotations.ClientSend());
            Record(trace, Annotations.ClientRecv());

            _spanBatcher.Verify(b => b.LogSpan(It.IsAny<Span>()), Times.Once());
        }

        [Test]
        public void SpansAreLoggedAfterEndAnnotationServerSend()
        {
            var trace = Trace.Create();

            Record(trace, Annotations.ServerRecv());
            Record(trace, Annotations.ServerSend());

            _spanBatcher.Verify(b => b.LogSpan(It.IsAny<Span>()), Times.Once());

        }

        [Test]
        public void SpansShouldBeFlushedAfterTtl()
        {
            var now = TimeUtils.UtcNow;

            var firstSpanState = new SpanState(traceId: 1, parentSpanId: 0, spanId: 4874542152, flags: SpanFlags.None);
            var record = new Record(firstSpanState, now, Annotations.ServerRecv());

            _tracer.Record(record);

            // futureTime = now + (ttl - 4)
            var futureTime = now.AddSeconds(ZipkinTracer.TimeToLive.TotalSeconds - 4); // of course test will fail if TTL is set lower than 4 seconds

            _tracer.FlushOldSpans(futureTime); // shouldn't do anything since we haven't reached span ttl yet

            _spanBatcher.Verify(b => b.LogSpan(It.IsAny<Span>()), Times.Never);

            var newerSpanState = new SpanState(traceId: 2, parentSpanId: 0, spanId: 9988415021, flags: SpanFlags.None);
            var newerRecord = new Record(newerSpanState, futureTime, Annotations.ServerRecv());
            _tracer.Record(newerRecord); // creates a second span

            futureTime = futureTime.AddSeconds(5); // = now + (ttl - 4) + 5 = now + ttl + 1

            _tracer.FlushOldSpans(futureTime); // should flush only the first span since we are 1 second past its TTL but 5 seconds before the second span TTL

            _spanBatcher.Verify(b => b.LogSpan(It.IsAny<Span>()), Times.Once());

            // "ServerSend" should make the second span "complete" hence the second span should be sent immediately
            var newerComplementaryRecord = new Record(newerSpanState, futureTime, Annotations.ServerSend());
            _tracer.Record(newerComplementaryRecord);

            _spanBatcher.Verify(b => b.LogSpan(It.IsAny<Span>()), Times.Exactly(2));
        }

        [Test]
        public void StatisticsAreUpdatedForFlush()
        {
            var trace = Trace.Create();

            var now = TimeUtils.UtcNow;
            var record = new Record(trace.CurrentSpan, now.AddSeconds(-2 * ZipkinTracer.TimeToLive.TotalSeconds), Annotations.ServerRecv());
            _tracer.Record(record);
            _tracer.FlushOldSpans(now);

            _statistics.Verify(s => s.UpdateSpanFlushed(), Times.Once());
        }


        [Test]
        public void StatisticsAreUpdatedForRecord()
        {
            var trace = Trace.Create();

            Record(trace, Annotations.ServerRecv());

            _statistics.Verify(s => s.UpdateRecordProcessed(), Times.Once());
        }

        private void Record(Trace trace, IAnnotation annotation)
        {
            var record = new Record(trace.CurrentSpan, TimeUtils.UtcNow, annotation);
            _tracer.Record(record);
        }

    }
}
