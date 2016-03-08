using System;
using System.IO;
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
        private readonly SpanState _spanState = new SpanState(1, null, 5, Flags.Empty);

        private Mock<ISpanSerializer> _spanSerializer;
        private Mock<IZipkinSender> _spanSender;
        private Mock<IStatistics> _statistics;
        private ZipkinTracer _tracer;

        [SetUp]
        public void Setup()
        {
            _spanSerializer = new Mock<ISpanSerializer>();
            _spanSender = new Mock<IZipkinSender>();
            _statistics = new Mock<IStatistics>();
            _tracer = new ZipkinTracer(_spanSender.Object, _spanSerializer.Object, _statistics.Object);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowWithNullSender()
        {
            var tracer = new ZipkinTracer(null);
        }

        [Test]
        public void SpansAreLoggedAfterEndAnnotationClientRecv()
        {
            var trace = Trace.CreateFromId(_spanState);

            Record(trace, Annotations.ClientSend());
            Record(trace, Annotations.ClientRecv());

            _spanSerializer.Verify(s => s.SerializeTo(It.IsAny<Stream>(), It.IsAny<Span>()), Times.Once());
            _spanSender.Verify(sender => sender.Send(It.IsAny<byte[]>()), Times.Once());
        }

        [Test]
        public void SpansAreLoggedAfterEndAnnotationServerSend()
        {
            var trace = Trace.CreateFromId(_spanState);

            Record(trace, Annotations.ServerRecv());
            Record(trace, Annotations.ServerSend());

            _spanSerializer.Verify(s => s.SerializeTo(It.IsAny<Stream>(), It.IsAny<Span>()), Times.Once());
            _spanSender.Verify(sender => sender.Send(It.IsAny<byte[]>()), Times.Once());

        }

        [Test]
        public void SpansShouldBeFlushedAfterTtl()
        {
            var now = TimeUtils.UtcNow;

            var firstSpanState = new SpanState(traceId: 1, parentSpanId: 0, spanId: 4874542152, flags: Flags.Empty);
            var record = new Record(firstSpanState, now, Annotations.ServerRecv());

            _tracer.Record(record);

            // futureTime = now + (ttl - 4)
            var futureTime = now.AddSeconds(ZipkinTracer.TimeToLive - 4); // of course test will fail if TTL is set lower than 4 seconds

            _tracer.FlushOldSpans(futureTime); // shouldn't do anything since we haven't reached span ttl yet

            _spanSender.Verify(sender => sender.Send(It.IsAny<byte[]>()), Times.Never());

            var newerSpanState = new SpanState(traceId: 2, parentSpanId: 0, spanId: 9988415021, flags: Flags.Empty);
            var newerRecord = new Record(newerSpanState, futureTime, Annotations.ServerRecv());
            _tracer.Record(newerRecord); // creates a second span

            futureTime = futureTime.AddSeconds(5); // = now + (ttl - 4) + 5 = now + ttl + 1

            _tracer.FlushOldSpans(futureTime); // should flush only the first span since we are 1 second past its TTL but 5 seconds before the second span TTL

            _spanSender.Verify(sender => sender.Send(It.IsAny<byte[]>()), Times.Once());

            // "ServerSend" should make the second span "complete" hence the second span should be sent immediately
            var newerComplementaryRecord = new Record(newerSpanState, futureTime, Annotations.ServerSend());
            _tracer.Record(newerComplementaryRecord);

            _spanSender.Verify(sender => sender.Send(It.IsAny<byte[]>()), Times.Exactly(2));
        }

        [Test]
        public void StatisticsAreUpdatedForFlush()
        {

            var record = new Record(_spanState, TimeUtils.UtcNow.AddSeconds(-2 * ZipkinTracer.TimeToLive), Annotations.ServerRecv());
            _tracer.Record(record);
            _tracer.FlushOldSpans(TimeUtils.UtcNow);

            _statistics.Verify(s => s.UpdateSpanFlushed(), Times.Once());
        }


        [Test]
        public void StatisticsAreUpdatedForSent()
        {
            var trace = Trace.CreateFromId(_spanState);

            Record(trace, Annotations.ServerSend());

            _statistics.Verify(s => s.UpdateSpanSent(), Times.Once());
            _statistics.Verify(s => s.UpdateSpanSentBytes(It.IsAny<int>()), Times.Once());
        }

        [Test]
        public void StatisticsAreUpdatedForRecord()
        {
            var trace = Trace.CreateFromId(_spanState);

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
