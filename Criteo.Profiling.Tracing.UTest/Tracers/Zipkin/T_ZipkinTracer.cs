using System;
using Criteo.Profiling.Tracing.Annotation;
using Criteo.Profiling.Tracing.Tracers.Zipkin;
using Criteo.Profiling.Tracing.Utils;
using Moq;
using NUnit.Framework;

namespace Criteo.Profiling.Tracing.UTest.Tracers.Zipkin
{
    [TestFixture]
    class T_ZipkinTracer
    {

        private readonly SpanState _spanState = new SpanState(1, null, 2, Flags.Empty);

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowWithNullSender()
        {
            var tracer = new ZipkinTracer(null);
        }

        [Test]
        public void SpansAreLoggedAfterEndAnnotationClientRecv()
        {
            var mockedSender = new Mock<IZipkinSender>();
            var zipkinTracer = new ZipkinTracer(mockedSender.Object);

            var trace = Trace.CreateFromId(_spanState);

            Record(zipkinTracer, trace, Annotations.ClientSend());
            Record(zipkinTracer, trace, Annotations.ClientRecv());

            mockedSender.Verify(sender => sender.Send(It.IsAny<byte[]>()), Times.Once());
            Assert.AreEqual(2, zipkinTracer.Statistics.RecordProcessed);
            Assert.AreEqual(1, zipkinTracer.Statistics.SpanSent);
            Assert.AreEqual(0, zipkinTracer.Statistics.SpanFlushed);
            Assert.IsTrue(zipkinTracer.Statistics.SpanSentTotalBytes > 0);
        }

        [Test]
        public void SpansAreLoggedAfterEndAnnotationServerSend()
        {
            var mockedSender = new Mock<IZipkinSender>();
            var zipkinTracer = new ZipkinTracer(mockedSender.Object);

            var trace = Trace.CreateFromId(_spanState);

            Record(zipkinTracer, trace, Annotations.ServerRecv());
            Record(zipkinTracer, trace, Annotations.ServerSend());

            mockedSender.Verify(sender => sender.Send(It.IsAny<byte[]>()), Times.Once());
            Assert.AreEqual(2, zipkinTracer.Statistics.RecordProcessed);
            Assert.AreEqual(1, zipkinTracer.Statistics.SpanSent);
            Assert.AreEqual(0, zipkinTracer.Statistics.SpanFlushed);
            Assert.IsTrue(zipkinTracer.Statistics.SpanSentTotalBytes > 0);
        }

        [Test]
        public void SpansShouldBeFlushedAfterTtl()
        {
            var mockedSender = new Mock<IZipkinSender>();
            var zipkinTracer = new ZipkinTracer(mockedSender.Object);

            var now = TimeUtils.UtcNow;

            var firstSpanState = new SpanState(traceId: 1, parentSpanId: 0, spanId: 4874542152, flags: Flags.Empty);
            var record = new Record(firstSpanState, now, Annotations.ServerRecv());

            zipkinTracer.Record(record);

            // futureTime = now + (ttl - 4)
            var futureTime = now.AddSeconds(ZipkinTracer.TimeToLive - 4); // of course test will fail if TTL is set lower than 4 seconds

            zipkinTracer.FlushOldSpans(futureTime); // shouldn't do anything since we haven't reached span ttl yet

            mockedSender.Verify(sender => sender.Send(It.IsAny<byte[]>()), Times.Never());

            var newerSpanState = new SpanState(traceId: 2, parentSpanId: 0, spanId: 9988415021, flags: Flags.Empty);
            var newerRecord = new Record(newerSpanState, futureTime, Annotations.ServerRecv());
            zipkinTracer.Record(newerRecord); // creates a second span

            futureTime = futureTime.AddSeconds(5); // = now + (ttl - 4) + 5 = now + ttl + 1

            zipkinTracer.FlushOldSpans(futureTime); // should flush only the first span since we are 1 second past its TTL but 5 seconds before the second span TTL

            mockedSender.Verify(sender => sender.Send(It.IsAny<byte[]>()), Times.Once());

            // "ServerSend" should make the second span "complete" hence the second span should be sent immediately
            var newerComplementaryRecord = new Record(newerSpanState, futureTime, Annotations.ServerSend());
            zipkinTracer.Record(newerComplementaryRecord);

            mockedSender.Verify(sender => sender.Send(It.IsAny<byte[]>()), Times.Exactly(2));

            Assert.AreEqual(3, zipkinTracer.Statistics.RecordProcessed);
            Assert.AreEqual(2, zipkinTracer.Statistics.SpanSent);
            Assert.AreEqual(1, zipkinTracer.Statistics.SpanFlushed);
            Assert.IsTrue(zipkinTracer.Statistics.SpanSentTotalBytes > 0);
        }

        [Test]
        public void StatisticsAreUpdatedForFlush()
        {
            var mockedSender = new Mock<IZipkinSender>();
            var statistics = new Mock<IStatistics>();
            var zipkinTracer = new ZipkinTracer(mockedSender.Object, statistics.Object);

            var record = new Record(_spanState, TimeUtils.UtcNow.AddSeconds(-2 * ZipkinTracer.TimeToLive), Annotations.ServerRecv());
            zipkinTracer.Record(record);
            zipkinTracer.FlushOldSpans(TimeUtils.UtcNow);

            statistics.Verify(s => s.UpdateSpanFlushed(), Times.Once());
        }


        [Test]
        public void StatisticsAreUpdatedForSent()
        {
            var mockedSender = new Mock<IZipkinSender>();
            var statistics = new Mock<IStatistics>();
            var zipkinTracer = new ZipkinTracer(mockedSender.Object, statistics.Object);

            var trace = Trace.CreateFromId(_spanState);

            Record(zipkinTracer, trace, Annotations.ServerSend());

            statistics.Verify(s => s.UpdateSpanSent(), Times.Once());
            statistics.Verify(s => s.UpdateSpanSentBytes(It.Is<int>(n => n > 0)), Times.Once());
        }

        [Test]
        public void StatisticsAreUpdatedForRecord()
        {
            var mockedSender = new Mock<IZipkinSender>();
            var statistics = new Mock<IStatistics>();
            var zipkinTracer = new ZipkinTracer(mockedSender.Object, statistics.Object);

            var trace = Trace.CreateFromId(_spanState);

            Record(zipkinTracer, trace, Annotations.ServerRecv());
            statistics.Verify(s => s.UpdateRecordProcessed(), Times.Once());
        }

        private static void Record(ITracer tracer, Trace trace, IAnnotation annotation)
        {
            var recordClientSend = new Record(trace.CurrentSpan, TimeUtils.UtcNow, annotation);
            tracer.Record(recordClientSend);
        }

    }
}
