using System;
using System.Linq;
using Moq;
using NUnit.Framework;
using zipkin4net.Internal.Recorder;
using zipkin4net.Tracers.Zipkin;
using Span = zipkin4net.Internal.V2.Span;

namespace zipkin4net.UTest.Internal.Reporter
{
    [TestFixture]
    public class T_Recorder
    {
        private IRecorder _recorder;

        private readonly Endpoint _endpoint =
            new Endpoint(SerializerUtils.DefaultServiceName, SerializerUtils.DefaultEndPoint);

        private Mock<IReporter<Span>> _mockReporter;

        [SetUp]
        public void SetUp()
        {
            _mockReporter = new Mock<IReporter<zipkin4net.Internal.V2.Span>>();
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
        public void FlushShouldSendIt()
        {
            var span = CreateSpan();

            _recorder.Start(span);
            _recorder.Flush(span);

            _mockReporter.Verify(r => r.Report(It.Is<Span>(spanToSerialize =>
                    spanToSerialize.Annotations.Any(a => a.Value.Equals("flush.timeout")))),
                Times.Once());
        }

        [Test]
        public void FlushingTwiceASpanShouldThrow()
        {
            var span = CreateSpan();

            _recorder.Start(span);
            _recorder.Flush(span);
            Assert.Throws<InvalidOperationException>(() => _recorder.Flush(span));
        }

        [TestCase(SpanKind.Client)]
        [TestCase(SpanKind.Server)]
        [TestCase(SpanKind.Producer)]
        [TestCase(SpanKind.Consumer)]
        public void FinishClientSpanShouldSetKind(SpanKind kind)
        {
            var span = CreateSpan();

            _recorder.Start(span);
            _recorder.Kind(span, kind);
            _recorder.Finish(span);

            Span.SpanKind expectedKind;
            Enum.TryParse(kind.ToString(), out expectedKind);
            
            _mockReporter.Verify(r => r.Report(It.Is<Span>(spanToSerialize =>
                spanToSerialize.Kind.Equals(expectedKind))), Times.Once());
        }

        [Test]
        public void FinishSpanWithoutKindShouldNotAddAnnotations()
        {
            var span = CreateSpan();

            _recorder.Start(span);
            _recorder.Finish(span);

            _mockReporter.Verify(
                r => r.Report(It.Is<Span>(spanToSerialize => spanToSerialize.Duration != 0L && spanToSerialize.Tags.Count == 0)), Times.Once());
        }

        private static ITraceContext CreateSpan()
        {
            return new SpanState(new Random().Next(), null, 1, false, false);
        }
    }
}