using System;
using zipkin4net.Annotation;
using zipkin4net.Tracers.Zipkin;
using zipkin4net.Utils;
using Moq;
using NUnit.Framework;
using zipkin4net.Internal.Recorder;

namespace zipkin4net.UTest.Tracers.Zipkin
{
    [TestFixture]
    internal class T_ZipkinTracer
    {
        private Mock<IReporter> _reporter;
        private ZipkinTracer _tracer;

        [SetUp]
        public void Setup()
        {
            _reporter = new Mock<IReporter>();
            _tracer = new ZipkinTracer(_reporter.Object, null);
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

            _reporter.Verify(r => r.Report(It.IsAny<Span>()), Times.Once());
        }

        [Test]
        public void SpansAreLoggedAfterEndAnnotationServerSend()
        {
            var trace = Trace.Create();

            Record(trace, Annotations.ServerRecv());
            Record(trace, Annotations.ServerSend());

            _reporter.Verify(r => r.Report(It.IsAny<Span>()), Times.Once());
        }

        [Test]
        public void StatisticsAreUpdatedForSent()
        {
            var trace = Trace.Create();

            Record(trace, Annotations.ServerSend());

            _reporter.Verify(r => r.Report(It.IsAny<Span>()), Times.Once());
        }

        [Test]
        public void StatisticsAreUpdatedForRecord()
        {
            var statistics = new Mock<IStatistics>();
            var tracer = new ZipkinTracer(Mock.Of<IZipkinSender>(), Mock.Of<ISpanSerializer>(), statistics.Object);
            var trace = Trace.Create();

            Record(tracer, trace, Annotations.ServerRecv());

            statistics.Verify(s => s.UpdateRecordProcessed(), Times.Once());
        }

        private void Record(Trace trace, IAnnotation annotation)
        {
            Record(_tracer, trace, annotation);
        }
        
        private static void Record(ITracer tracer, Trace trace, IAnnotation annotation)
        {
            var record = new Record(trace.CurrentSpan, TimeUtils.UtcNow, annotation);
            tracer.Record(record);
        }

    }
}
