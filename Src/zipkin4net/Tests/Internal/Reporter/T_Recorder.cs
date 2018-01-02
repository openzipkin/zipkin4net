using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using zipkin4net.Internal.Recorder;
using zipkin4net.Tracers.Zipkin;
using zipkin4net.Tracers.Zipkin.Thrift;
using Span = zipkin4net.Tracers.Zipkin.Span;

namespace zipkin4net.UTest.Internal.Reporter
{
    [TestFixture]
    public class T_Recorder
    {
        private IRecorder _recorder;
        private readonly IEndPoint _endpoint = new EndPoint(SerializerUtils.DefaultServiceName, SerializerUtils.DefaultEndPoint);
        private Mock<IReporter> _mockReporter;

        [SetUp]
        public void SetUp()
        {
            _mockReporter = new Mock<IReporter>();
            _recorder = new Recorder(_endpoint, _mockReporter.Object);
        }

        [Test]
        public void SpansAreLoggedAfterFinish()
        {
            var span = CreateSpan();

            _recorder.Start(span);
            _recorder.Finish(span);

            _mockReporter.Verify(r => r.Report(It.IsAny<Span>()), Times.Once());
        }

        [Test]
        public void SpansAreNotLoggedIfFinishIsNotCalled()
        {
            var span = CreateSpan();

            _recorder.Start(span);

            _mockReporter.Verify(r => r.Report(It.IsAny<Span>()), Times.Never());
        }

        [Test]
        public void FinishingTwiceASpanShouldThrow()
        {
            var span = CreateSpan();

            _recorder.Start(span);
            _recorder.Finish(span);
            Assert.Throws<InvalidOperationException>(() => _recorder.Finish(span));
        }

        [Test]
        public void FinishingANonStartedSpanShouldThrow()
        {
            var span = CreateSpan();

            Assert.Throws<InvalidOperationException>(() => _recorder.Finish(span));
        }

        [Test]
        public void FlushShouldMarkSpanCompleteAndSendIt()
        {
            var span = CreateSpan();

            _recorder.Start(span);
            _recorder.Flush(span);

            _mockReporter.Verify(r => r.Report(It.Is<Span>(spanToSerialize =>
                spanToSerialize.Complete && spanToSerialize.Annotations.Any(a => a.Value.Equals("flush.timeout")))), Times.Once());
        }

        [Test]
        public void FlushingTwiceASpanShouldThrow()
        {
            var span = CreateSpan();

            _recorder.Start(span);
            _recorder.Flush(span);
            Assert.Throws<InvalidOperationException>(() => _recorder.Flush(span));
        }

        [TestCase(SpanKind.Client, zipkinCoreConstants.CLIENT_SEND, zipkinCoreConstants.CLIENT_RECV)]
        [TestCase(SpanKind.Server, zipkinCoreConstants.SERVER_RECV, zipkinCoreConstants.SERVER_SEND)]
        [TestCase(SpanKind.Producer, zipkinCoreConstants.MESSAGE_SEND)]
        [TestCase(SpanKind.Consumer, zipkinCoreConstants.MESSAGE_RECV)]
        public void FinishClientSpanShouldAddAnnotations(SpanKind kind, params string[] annotations)
        {
            var span = CreateSpan();

            _recorder.Start(span);
            _recorder.Kind(span, kind);
            _recorder.Finish(span);

            _mockReporter.Verify(r => r.Report(It.Is<Span>(spanToSerialize =>
                spanToSerialize.Complete &&
                AnnotationsContainsExactly(spanToSerialize.Annotations, annotations))), Times.Once());
        }

        private static bool AnnotationsContainsExactly(ICollection<ZipkinAnnotation> annotations,
            params string[] expectedAnnotations)
        {
            return annotations.Count == expectedAnnotations.Length && Array.TrueForAll(expectedAnnotations,
                       annotation => annotations.Any(b => b.Value.Equals(annotation)));
        }

        [Test]
        public void FinishSpanWithoutKindShouldNotAddAnnotations()
        {
            var span = CreateSpan();

            _recorder.Start(span);
            _recorder.Finish(span);

            _mockReporter.Verify(r => r.Report(It.Is<Span>(spanToSerialize => spanToSerialize.Complete && spanToSerialize.BinaryAnnotations.Count == 0)), Times.Once());
        }

        private static ITraceContext CreateSpan()
        {
            return new SpanState(new Random().Next(), null, 1, false, false);
        }
    }
}