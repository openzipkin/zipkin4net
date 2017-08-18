using System;
using System.Diagnostics;
using System.Threading;
using zipkin4net.Tracers.Zipkin;
using NUnit.Framework;
using Moq;

namespace zipkin4net.UTest.Tracers.Zipkin
{
    [TestFixture]
    internal class T_RateLimiter
    {
        private byte[] _data;
        private Mock<IZipkinSender> _underlyingSender;
        private Mock<ILogger> _logger;

        [SetUp]
        public void Setup()
        {
            _data = new byte[] { 0x01, 0x02 };
            _underlyingSender = new Mock<IZipkinSender>();
            _logger = new Mock<ILogger>();
            TraceManager.Start(_logger.Object);
        }

        [TearDown]
        public void TearDown()
        {
            TraceManager.Stop();
        }


        [Test]
        public void SendsNothingWhenMaxReqIsZero()
        {
            var sender = new RateLimiterZipkinSender(_underlyingSender.Object, 0);

            sender.Send(_data);

            _underlyingSender.Verify(zipkinSender => zipkinSender.Send(It.Is<byte[]>(bytes => bytes == _data)), Times.Never());
        }

        [TestCase(0)]
        [TestCase(10)]
        [TestCase(20)]
        public void RequestsAreNotThrottledIfMaxReqIsNotReached(int maxReq)
        {
            var sender = new RateLimiterZipkinSender(_underlyingSender.Object, maxReq, TimeSpan.FromSeconds(5));

            for (var i = 0; i < maxReq + 1; i++)
                sender.Send(_data);

            _underlyingSender.Verify(zipkinSender => zipkinSender.Send(It.Is<byte[]>(bytes => bytes == _data)), Times.Exactly(maxReq));
        }

        [TestCase(2, 100, 1)]
        [TestCase(15, 100, 10)]
        [TestCase(10, 10, 1)]
        [TestCase(10, 10, 100)]
        public void ExcessiveRequestsAreThrottledOnLongTerm(int maxSendRequest, int perSpanMs, int numberOfSpan)
        {
            var perSpan = TimeSpan.FromMilliseconds(perSpanMs);
            var sender = new RateLimiterZipkinSender(_underlyingSender.Object, maxSendRequest, perSpan);

            var watch = Stopwatch.StartNew();
            while (true)
            {
                if (watch.ElapsedMilliseconds >= perSpan.TotalMilliseconds * numberOfSpan)
                    break;
                sender.Send(_data);
            }

            var tolerance =  1.5 / numberOfSpan + 1.05;
            var trueSendExpect = numberOfSpan * maxSendRequest;
            var sendExpectationWithBurstTolerance = (int) Math.Ceiling(trueSendExpect * tolerance);
            _underlyingSender.Verify(zipkinSender => zipkinSender.Send(It.Is<byte[]>(bytes => bytes == _data)), Times.AtMost(sendExpectationWithBurstTolerance));
        }

        [Test]
        public void ShouldLogAtLeastOnceWhenMaxReqIsZero()
        {
            var duration = TimeSpan.FromMilliseconds(10);
            var sender = new RateLimiterZipkinSender(_underlyingSender.Object, 0, duration);

            sender.Send(_data);
            Thread.Sleep(TimeSpan.FromMilliseconds(duration.TotalMilliseconds * 10 * 1.05));
            sender.Send(_data);

            _logger.Verify(logger => logger.LogWarning(It.IsAny<string>()), Times.AtLeastOnce());
        }
    }
}
