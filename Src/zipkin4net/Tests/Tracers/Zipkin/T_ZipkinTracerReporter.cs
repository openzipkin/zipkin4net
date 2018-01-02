using System;
using System.IO;
using Moq;
using NUnit.Framework;
using zipkin4net.Tracers.Zipkin;
using zipkin4net.Utils;

namespace zipkin4net.UTest.Tracers.Zipkin
{
    [TestFixture]
    public class T_ZipkinTracerReporter
    {
        [Test]
        public void ShouldThrowIfSenderIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new ZipkinTracerReporter(null, Mock.Of<ISpanSerializer>(), Mock.Of<IStatistics>()));
        }
        
        [Test]
        public void ShouldThrowIfSerializerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new ZipkinTracerReporter(Mock.Of<IZipkinSender>(), null, Mock.Of<IStatistics>()));
        }
        
        [Test]
        public void ShouldThrowIfStatisticsIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new ZipkinTracerReporter(Mock.Of<IZipkinSender>(), Mock.Of<ISpanSerializer>(), null));
        }

        [Test]
        public void ReportShouldSerializeAndUpdateMetrics()
        {
            var mockSender = new Mock<IZipkinSender>();
            var mockSerializer = new Mock<ISpanSerializer>();
            var mockStatistics = new Mock<IStatistics>();
            var reporter = new ZipkinTracerReporter(mockSender.Object, mockSerializer.Object, mockStatistics.Object);
            var span = new Span(Mock.Of<ITraceContext>(), TimeUtils.UtcNow);

            reporter.Report(span);

            mockSerializer.Verify(s => s.SerializeTo(It.IsAny<Stream>(), It.IsAny<Span>()), Times.Once());
            mockSender.Verify(sender => sender.Send(It.IsAny<byte[]>()), Times.Once());

            mockStatistics.Verify(statistics => statistics.UpdateSpanSent(), Times.Once());
            mockStatistics.Verify(statistics => statistics.UpdateSpanSentBytes(It.IsAny<int>()), Times.Once());
        }
    }
}