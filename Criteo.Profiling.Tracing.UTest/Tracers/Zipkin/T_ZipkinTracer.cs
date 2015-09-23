using System;
using Criteo.Profiling.Tracing.Tracers.Zipkin;
using Moq;
using NUnit.Framework;

namespace Criteo.Profiling.Tracing.UTest.Tracers.Zipkin
{
    [TestFixture]
    class T_ZipkinTracer
    {

        [SetUp]
        public void EnableAndClearTracers()
        {
            Trace.TracingEnabled = true;
            Tracer.Clear();
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
            var mockedSender = new Mock<IZipkinSender>();
            var zipkinTracer = new ZipkinTracer(mockedSender.Object);

            Tracer.Register(zipkinTracer);

            var trace = Trace.Create();

            trace.Record(Annotations.ClientSend());
            trace.Record(Annotations.ClientRecv());

            mockedSender.Verify(sender => sender.Send(It.IsAny<byte[]>()), Times.Once());
        }

        [Test]
        public void SpansAreLoggedAfterEndAnnotationServerSend()
        {
            var mockedSender = new Mock<IZipkinSender>();
            var zipkinTracer = new ZipkinTracer(mockedSender.Object);

            Tracer.Register(zipkinTracer);

            var trace = Trace.Create();

            trace.Record(Annotations.ServerRecv());
            trace.Record(Annotations.ServerSend());

            mockedSender.Verify(sender => sender.Send(It.IsAny<byte[]>()), Times.Once());
        }

        [Test]
        public void RecordsShouldBeFlushedAfterTtl()
        {
            var mockedSender = new Mock<IZipkinSender>();
            var zipkinTracer = new ZipkinTracer(mockedSender.Object);

            var now = DateTime.UtcNow;

            var firstSpanId = new SpanId(traceId: 1, parentSpanId: 0, id: 4874542152, flags: null);
            var record = new Record(firstSpanId, now, Annotations.ServerRecv(), 0);

            zipkinTracer.Record(record);

            // futureTime = now + (ttl - 10)
            var futureTime = now.AddSeconds(ZipkinTracer.TimeToLive - 10); // of course test will fail if TTL is set lower than 10 seconds

            zipkinTracer.FlushOldSpans(futureTime); // shouldn't do anything since we haven't reached span ttl yet

            mockedSender.Verify(sender => sender.Send(It.IsAny<byte[]>()), Times.Never());

            var newerSpanId = new SpanId(traceId: 2, parentSpanId: 0, id: 9988415021, flags: null);
            var newerRecord = new Record(newerSpanId, futureTime, Annotations.ServerRecv(), 0);
            zipkinTracer.Record(newerRecord); // creates a second span

            futureTime = futureTime.AddSeconds(15); // = now + (ttl + 5)

            zipkinTracer.FlushOldSpans(futureTime); // should flush only the first span since we are 5 seconds past its TTL

            mockedSender.Verify(sender => sender.Send(It.IsAny<byte[]>()), Times.Once());

            // "ServerSend" should make the second span "complete" hence the second span should be sent immediately
            var newerComplementaryRecord = new Record(newerSpanId, futureTime, Annotations.ServerSend(), 0);
            zipkinTracer.Record(newerComplementaryRecord);

            mockedSender.Verify(sender => sender.Send(It.IsAny<byte[]>()), Times.Exactly(2));
        }

    }
}
