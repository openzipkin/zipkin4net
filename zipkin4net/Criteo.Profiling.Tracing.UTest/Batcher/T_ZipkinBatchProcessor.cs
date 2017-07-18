using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Criteo.Profiling.Tracing.Batcher;
using Criteo.Profiling.Tracing.Tracers.Zipkin;
using Criteo.Profiling.Tracing.Utils;
using Moq;
using NUnit.Framework;


namespace Criteo.Profiling.Tracing.UTest.Batcher
{
    [TestFixture]
    internal class T_ZipkinBatchProcessor
    {
        [SetUp]
        public void Setup()
        {
            _spanSerializer = new Mock<ISpanSerializer>();
            _spanSender = new Mock<IZipkinSender>();
            _statistics = new Mock<IStatistics>();
            _timewindow = TimeSpan.FromSeconds(1);
            _batcher = new ZipkinBatchSpanProcessor(_spanSender.Object, _spanSerializer.Object, _statistics.Object,
                TimeSpan.FromSeconds(1));
        }

        private Mock<ISpanSerializer> _spanSerializer;
        private Mock<IZipkinSender> _spanSender;
        private Mock<IStatistics> _statistics;
        private TimeSpan _timewindow;
        private ZipkinBatchSpanProcessor _batcher;


        private static Span CreateSpan()
        {
            return new Span(new SpanState(Guid.Empty, null, RandomUtils.NextLong(), SpanFlags.None), DateTime.UtcNow);
        }

        [TestCase(2)]
        [TestCase(4)]
        [TestCase(8)]
        public void ShouldNotSendSpansWhenBatchSizeNotReached(int batchSize)
        {
            _batcher = new ZipkinBatchSpanProcessor(_spanSender.Object, _spanSerializer.Object, _statistics.Object,
                _timewindow, batchSize);
            var span = CreateSpan();
            for (var i = 0; i < batchSize - 1; i++)
                _batcher.LogSpan(span);
            _spanSender.Verify(s => s.Send(It.IsAny<byte[]>()), Times.Never);
            _spanSerializer.Verify(s => s.SerializeTo(It.IsAny<Stream>(), It.IsAny<IEnumerable<Span>>()), Times.Never);
            _statistics.Verify(s => s.UpdateSpanSent(It.IsAny<int>()), Times.Never);
            _statistics.Verify(s => s.UpdateSpanSentBytes(It.IsAny<int>()), Times.Never);
        }


        [TestCase(2)]
        [TestCase(4)]
        [TestCase(8)]
        public void ShouldSendSpansWhenBatchSizeReached(int batchSize)
        {
            var numberOfBatch = 5;
            var count = 0;
            _batcher = new ZipkinBatchSpanProcessor(_spanSender.Object, _spanSerializer.Object, _statistics.Object,
                _timewindow, batchSize);
            var span = CreateSpan();
            _statistics.Setup(x => x.UpdateSpanSentBytes(It.IsAny<int>())).Callback(() =>
            {
                if (++count == numberOfBatch)
                {
                    lock (span)
                    {
                        Monitor.Pulse(span);
                    }
                }
            });
            for (var i = 0; i < numberOfBatch * batchSize; i++)
            {
                _batcher.LogSpan(span);
            }
            lock (span)
            {
                Monitor.Wait(span,500);
            }
            _spanSender.Verify(s => s.Send(It.IsAny<byte[]>()), Times.Exactly(numberOfBatch));
            _spanSerializer.Verify(s => s.SerializeTo(It.IsAny<Stream>(), It.IsAny<IEnumerable<Span>>()),
                Times.Exactly(numberOfBatch));
            _statistics.Verify(s => s.UpdateSpanSent(batchSize), Times.Exactly(numberOfBatch));
            _statistics.Verify(s => s.UpdateSpanSentBytes(It.IsAny<int>()), Times.Exactly(numberOfBatch));
        }

        [TestCase(-1)]
        [TestCase(0)]
        public void ShouldThrowWithInvalidBatchSize(int batchSize)
        {
            Assert.Throws<ArgumentException>(() =>
            {
                var batcher = new ZipkinBatchSpanProcessor(Mock.Of<IZipkinSender>(), Mock.Of<ISpanSerializer>(),
                    Mock.Of<IStatistics>(), _timewindow, batchSize);
            });
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void ShouldThrowWithTimeoutTooLow(int timeout)
        {
            var timeWindow = TimeSpan.FromSeconds(timeout);
            Assert.Throws<ArgumentException>(() =>
            {
                var batcher = new ZipkinBatchSpanProcessor(Mock.Of<IZipkinSender>(), Mock.Of<ISpanSerializer>(),
                    Mock.Of<IStatistics>(), timeWindow);
            });
        }

        [Test]
        public void ShouldSendSpansWhenTimeoutOccurs()
        {
            var batchSize = 10;
            var timeout = 100;
            _batcher = new ZipkinBatchSpanProcessor(_spanSender.Object, _spanSerializer.Object, _statistics.Object,
                TimeSpan.FromMilliseconds(timeout), batchSize);
            var span = CreateSpan();
            _spanSender.Setup(x => x.Send(It.IsAny<byte[]>())).Callback(() =>
            {
                lock (span)
                {
                    Monitor.Pulse(span);
                }
            });


            _batcher.LogSpan(span);
            _spanSender.Verify(s => s.Send(It.IsAny<byte[]>()), Times.Never);
            _spanSerializer.Verify(s => s.SerializeTo(It.IsAny<Stream>(), It.IsAny<IEnumerable<Span>>()), Times.Never);
            lock (span)
            {
                Monitor.Wait(span, 500);
            }
            _spanSender.Verify(s => s.Send(It.IsAny<byte[]>()), Times.Once);
            _spanSerializer.Verify(s => s.SerializeTo(It.IsAny<Stream>(), It.IsAny<IEnumerable<Span>>()), Times.Once);
        }

        [Test]
        public void ShouldThrowWithNullSender()
        {
            IZipkinSender sender = null;
            Assert.Throws<ArgumentNullException>(() =>
            {
                var batcher = new ZipkinBatchSpanProcessor(sender, Mock.Of<ISpanSerializer>(), Mock.Of<IStatistics>(),
                    _timewindow);
            });
        }

        [Test]
        public void ShouldThrowWithNullSerializer()
        {
            ISpanSerializer serializer = null;
            Assert.Throws<ArgumentNullException>(() =>
            {
                var batcher = new ZipkinBatchSpanProcessor(Mock.Of<IZipkinSender>(), serializer, Mock.Of<IStatistics>(),
                    _timewindow);
            });
        }

        [Test]
        public void ShouldThrowWithNullStatistics()
        {
            IStatistics statistics = null;
            Assert.Throws<ArgumentNullException>(() =>
            {
                var batcher = new ZipkinBatchSpanProcessor(Mock.Of<IZipkinSender>(), Mock.Of<ISpanSerializer>(),
                    statistics, _timewindow);
            });
        }
    }
}