using System;
using System.Linq;
using System.Net;
using Moq;
using zipkin4net.Annotation;
using zipkin4net.Tracers.Zipkin;
using zipkin4net.Utils;
using NUnit.Framework;
using zipkin4net.Internal.Recorder;
using Span = zipkin4net.Internal.V2.Span;

namespace zipkin4net.UTest.Tracers.Zipkin
{
    [TestFixture]
    internal class T_ZipkinAnnotationVisitor
    {
/*
        private static readonly Endpoint DefaultLocalEndPoint =
            new Endpoint(SerializerUtils.DefaultServiceName, SerializerUtils.DefaultEndPoint);
        private static readonly SpanState SpanState = new SpanState(1, 0, 2, isSampled: null, isDebug: false);
        private Recorder _recorder;
        private Mock<IReporter<Span>> _reporter = new Mock<IReporter<Span>>();

        [SetUp]
        public void SetUp()
        {
            _recorder = new Recorder(new Endpoint(SerializerUtils.DefaultServiceName, SerializerUtils.DefaultEndPoint),
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

            _reporter.Verify(r => r.Report(It.Is<Span>(s => "myService".Equals(s.LocalServiceName))));
        }

        [Test]
        public void LocalAddrAnnotationChangesSpanLocalAddr()
        {
            _recorder.Start(SpanState);
            var ipEndpoint = new IPEndPoint(IPAddress.Loopback, 9987);
            CreateAndVisitRecord(SpanState, Annotations.LocalAddr(ipEndpoint));
            _recorder.Finish(SpanState);

            _reporter.Verify(r => r.Report(It.Is<Span>(s => ipEndpoint.Equals(s.LocalEndpoint.IpEndPoint))));
        }

        [Test]
        [Description("RPC, ServiceName and LocalAddr annotations override any previous recorded value.")]
        public void LastAnnotationValueIsKeptIfMultipleRecord()
        {
            _recorder.Start(SpanState);
            CreateAndVisitRecord(SpanState, Annotations.ServiceName("myService"));
            CreateAndVisitRecord(SpanState, Annotations.ServiceName("someOtherName"));
            _recorder.Finish(SpanState);

            _reporter.Verify(r => r.Report(It.Is<Span>(s => "someOtherName".Equals(s.LocalServiceName))));
        }

        private void CreateAndVisitRecord(ITraceContext spanState, IAnnotation annotation)
        {
            var record = new Record(spanState, TimeUtils.UtcNow, annotation);
            var visitor = new ZipkinAnnotationVisitor(_recorder, record, spanState);

            record.Annotation.Accept(visitor);
        }
        

        [TestCase("string")]
        [TestCase(true)]
        [TestCase(short.MaxValue)]
        [TestCase(int.MaxValue)]
        [TestCase(long.MaxValue)]
        [TestCase(new byte[] {0x93})]
        [TestCase(9.3d)]
        public void TagAnnotationCorrectlyAdded(object value)
        {
            var span = new MutableSpan(SpanState, DefaultLocalEndPoint);

            _recorder.Start(SpanState);
            var record = new Record(SpanState, TimeUtils.UtcNow, new TagAnnotation("magicKey", value));
            var visitor = new ZipkinAnnotationVisitor(record, span);
            record.Annotation.Accept(visitor);
            _recorder.Finish(SpanState);

            _reporter.Verify(r => r.Report(It.Is<Span>(s =>
                s.Annotations.Count == 0 && s.Tags.Count == 1 && s.Tags.Any(b =>
                    "magicKey".Equals(b.Key) && value.ToString().Equals(b.Value)))));
        }


        [Test]
        public void UnsupportedTagAnnotationShouldThrow()
        {
            var span = new MutableSpan(SpanState, DefaultLocalEndPoint);

            var record = new Record(SpanState, TimeUtils.UtcNow, new TagAnnotation("magicKey", 1f));
            var visitor = new ZipkinAnnotationVisitor(record, span);
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
            var expectedDuration = endTime.Subtract(startTime).TotalMilliseconds * 1000.0;

            long? parentId = 0;
            if (isRootSpan)
                parentId = null;
            var spanState = new SpanState(1, parentId, 2, isSampled: null, isDebug: false);
            var spanCreatedTimestamp = TimeUtils.UtcNow;
            var span = new MutableSpan(SpanState, DefaultLocalEndPoint);

            var recordStart = new Record(spanState, startTime, start);
            var reporter = new Mock<IReporter>();
            var visitorStart = new ZipkinAnnotationVisitor(recordStart, span);
            var recordStop = new Record(spanState, endTime, stop);
            var visitorStop = new ZipkinAnnotationVisitor(recordStop, span);

            recordStart.Annotation.Accept(visitorStart);
            reporter.Verify(r => r.Report(It.IsAny<Span>()), Times.Never());
            recordStop.Annotation.Accept(visitorStop);
            reporter.Verify(
                r => r.Report(It.Is<Span>(s =>
                    DurationComputedWhenSetAsComplete(s, isSpanStartedAndDurationSet, expectedDuration, startTime))),
                Times.Once());
        }

        private static bool DurationComputedWhenSetAsComplete(Span span, bool isSpanStartedAndDurationSet,
            long expectedDuration, DateTime startTime)
        {
            if (span.Duration == 0L)
            {
                return false;
            }

            if (isSpanStartedAndDurationSet)
            {
                return expectedDuration.Equals(span.Duration) &&
                       startTime.Equals(span.Timestamp);
            }
            else
            {
                return span.Duration == 0L &&
                       span.Timestamp == default(DateTime);
            }
        }*/
    }
}