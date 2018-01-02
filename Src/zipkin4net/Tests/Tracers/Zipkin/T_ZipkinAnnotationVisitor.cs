using System;
using System.Linq;
using System.Net;
using Moq;
using zipkin4net.Annotation;
using zipkin4net.Tracers.Zipkin;
using zipkin4net.Tracers.Zipkin.Thrift;
using zipkin4net.Utils;
using NUnit.Framework;
using zipkin4net.Internal.Recorder;
using Span = zipkin4net.Tracers.Zipkin.Span;

namespace zipkin4net.UTest.Tracers.Zipkin
{
    [TestFixture]
    internal class T_ZipkinAnnotationVisitor
    {
        private static readonly SpanState SpanState = new SpanState(1, 0, 2, isSampled: null, isDebug: false);
        private Recorder _recorder;
        private Mock<IReporter> _reporter = new Mock<IReporter>();

        [SetUp]
        public void SetUp()
        {
            _recorder = new Recorder(new EndPoint(SerializerUtils.DefaultServiceName, SerializerUtils.DefaultEndPoint),
                _reporter.Object);
        }

        [Test]
        public void RpcNameAnnotationChangesSpanName()
        {
            _recorder.Start(SpanState);
            CreateAndVisitRecord(SpanState, Annotations.Rpc("myRPCmethod"));
            _recorder.Finish(SpanState);


            _reporter.Verify(r => r.Report(It.Is<Span>(s => "myRPCmethod".Equals(s.Name))));
        }

        [Test]
        public void ServNameAnnotationChangesSpanServName()
        {
            _recorder.Start(SpanState);
            CreateAndVisitRecord(SpanState, Annotations.ServiceName("myService"));
            _recorder.Finish(SpanState);

            _reporter.Verify(r => r.Report(It.Is<Span>(s => "myService".Equals(s.ServiceName))));
        }

        [Test]
        public void LocalAddrAnnotationChangesSpanLocalAddr()
        {
            _recorder.Start(SpanState);
            var ipEndpoint = new IPEndPoint(IPAddress.Loopback, 9987);
            CreateAndVisitRecord(SpanState, Annotations.LocalAddr(ipEndpoint));
            _recorder.Finish(SpanState);

            _reporter.Verify(r => r.Report(It.Is<Span>(s => ipEndpoint.Equals(s.Endpoint))));
        }

        [Test]
        [Description("RPC, ServiceName and LocalAddr annotations override any previous recorded value.")]
        public void LastAnnotationValueIsKeptIfMultipleRecord()
        {
            _recorder.Start(SpanState);
            CreateAndVisitRecord(SpanState, Annotations.ServiceName("myService"));
            CreateAndVisitRecord(SpanState, Annotations.ServiceName("someOtherName"));
            _recorder.Finish(SpanState);

            _reporter.Verify(r => r.Report(It.Is<Span>(s => "someOtherName".Equals(s.ServiceName))));
        }

        private void CreateAndVisitRecord(ITraceContext spanState, IAnnotation annotation)
        {
            var record = new Record(spanState, TimeUtils.UtcNow, annotation);
            var visitor = new ZipkinAnnotationVisitor(_recorder, record, spanState);

            record.Annotation.Accept(visitor);
        }

        [TestCase("string", new byte[] {0x73, 0x74, 0x72, 0x69, 0x6E, 0x67}, AnnotationType.STRING)]
        [TestCase(true, new byte[] {0x1}, AnnotationType.BOOL)]
        [TestCase(short.MaxValue, new byte[] {0x7F, 0xFF}, AnnotationType.I16)]
        [TestCase(int.MaxValue, new byte[] {0x7F, 0xFF, 0xFF, 0xFF}, AnnotationType.I32)]
        [TestCase(long.MaxValue, new byte[] {0x7F, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF}, AnnotationType.I64)]
        [TestCase(new byte[] {0x93}, new byte[] {0x93}, AnnotationType.BYTES)]
        [TestCase(9.3d, new byte[] {0x40, 0x22, 0x99, 0x99, 0x99, 0x99, 0x99, 0x9A,}, AnnotationType.DOUBLE)]
        public void TagAnnotationCorrectlyAdded(object value, byte[] expectedBytes, AnnotationType expectedType)
        {
            var span = new Span(SpanState, spanCreated: TimeUtils.UtcNow);

            _recorder.Start(SpanState);
            var record = new Record(SpanState, TimeUtils.UtcNow, new TagAnnotation("magicKey", value));
            var visitor = new ZipkinAnnotationVisitor(_recorder, record, span.SpanState);
            record.Annotation.Accept(visitor);
            _recorder.Finish(SpanState);

            _reporter.Verify(r => r.Report(It.Is<Span>(s =>
                s.Annotations.Count == 0 && s.BinaryAnnotations.Count == 1 && s.BinaryAnnotations.Any(b =>
                    "magicKey".Equals(b.Key) && expectedBytes.SequenceEqual(b.Value) &&
                    expectedType.Equals(b.AnnotationType)))));
        }

        [Test]
        public void UnsupportedTagAnnotationShouldThrow()
        {
            var span = new Span(SpanState, spanCreated: TimeUtils.UtcNow);

            var record = new Record(SpanState, TimeUtils.UtcNow, new TagAnnotation("magicKey", 1f));
            var visitor = new ZipkinAnnotationVisitor(_recorder, record, span.SpanState);
            _recorder.Start(SpanState);
            Assert.Throws<ArgumentException>(() => record.Annotation.Accept(visitor));
        }

        [Test]
        public void DurationAndSpanStartedSetWhenSetAsComplete()
        {
            VerifySpanDurationComputedWhenSetAsComplete(Annotations.ClientSend(), Annotations.ClientRecv(),
                isRootSpan: false, isSpanStartedAndDurationSet: true);
            VerifySpanDurationComputedWhenSetAsComplete(Annotations.ServerRecv(), Annotations.ServerSend(),
                isRootSpan: true, isSpanStartedAndDurationSet: true);
            VerifySpanDurationComputedWhenSetAsComplete(Annotations.ServerRecv(), Annotations.ServerSend(),
                isRootSpan: false, isSpanStartedAndDurationSet: false);
            VerifySpanDurationComputedWhenSetAsComplete(Annotations.LocalOperationStart("Operation"),
                Annotations.LocalOperationStop(), isRootSpan: false, isSpanStartedAndDurationSet: true);
            VerifySpanDurationComputedWhenSetAsComplete(Annotations.ProducerStart(), Annotations.ProducerStop(),
                isRootSpan: false, isSpanStartedAndDurationSet: false);
            VerifySpanDurationComputedWhenSetAsComplete(Annotations.ConsumerStart(), Annotations.ConsumerStop(),
                isRootSpan: false, isSpanStartedAndDurationSet: false);
        }

        private static void VerifySpanDurationComputedWhenSetAsComplete(IAnnotation start, IAnnotation stop,
            bool isRootSpan, bool isSpanStartedAndDurationSet)
        {
            var startTime = DateTime.Now;
            var endTime = startTime.AddHours(1);
            var expectedDuration = endTime.Subtract(startTime);

            long? parentId = 0;
            if (isRootSpan)
                parentId = null;
            var spanState = new SpanState(1, parentId, 2, isSampled: null, isDebug: false);
            var spanCreatedTimestamp = TimeUtils.UtcNow;
            var span = new Span(spanState, spanCreatedTimestamp);

            var recordStart = new Record(spanState, startTime, start);
            var reporter = new Mock<IReporter>();
            var recorder =
                new Recorder(new EndPoint(SerializerUtils.DefaultServiceName, SerializerUtils.DefaultEndPoint),
                    reporter.Object);
            var visitorStart = new ZipkinAnnotationVisitor(recorder, recordStart, span.SpanState);
            var recordStop = new Record(spanState, endTime, stop);
            var visitorStop = new ZipkinAnnotationVisitor(recorder, recordStop, span.SpanState);

            recordStart.Annotation.Accept(visitorStart);
            reporter.Verify(r => r.Report(It.IsAny<Span>()), Times.Never());
            recordStop.Annotation.Accept(visitorStop);
            reporter.Verify(
                r => r.Report(It.Is<Span>(s =>
                    DurationComputedWhenSetAsComplete(s, isSpanStartedAndDurationSet, expectedDuration, startTime))),
                Times.Once());
        }

        private static bool DurationComputedWhenSetAsComplete(Span span, bool isSpanStartedAndDurationSet,
            TimeSpan expectedDuration, DateTime startTime)
        {
            if (!span.Complete)
            {
                return false;
            }

            if (isSpanStartedAndDurationSet)
            {
                return expectedDuration.Equals(span.Duration) &&
                       startTime.Equals(span.SpanStarted);
            }
            else
            {
                return !span.Duration.HasValue &&
                       !span.SpanStarted.HasValue;
            }
        }
    }
}