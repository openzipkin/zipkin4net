using System;
using System.Linq;
using zipkin4net.Utils;
using Moq;
using NUnit.Framework;

namespace zipkin4net.UTest
{
    [TestFixture]
    internal class T_TraceManager
    {
        private Mock<ILogger> _mockLogger;

        [SetUp]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger>();
            TraceManager.Start(_mockLogger.Object);
        }

        [TearDown]
        public void TearDown()
        {
            TraceManager.Stop();
            TraceManager.ClearTracers();
        }

        [Test]
        public void TracersAreCorrectlyRegistered()
        {
            var tracer = new Mock<ITracer>();
            TraceManager.RegisterTracer(tracer.Object);

            Assert.AreEqual(1, TraceManager.Tracers.Count);
            Assert.AreEqual(tracer.Object, TraceManager.Tracers.First());

            var secondTracer = new Mock<ITracer>();

            TraceManager.RegisterTracer(secondTracer.Object);

            Assert.AreEqual(2, TraceManager.Tracers.Count);
            Assert.True(TraceManager.Tracers.Contains(tracer.Object));
            Assert.True(TraceManager.Tracers.Contains(secondTracer.Object));
        }

        [Test]
        public void TracersAreIndeedCleared()
        {
            var tracer = new Mock<ITracer>();
            TraceManager.RegisterTracer(tracer.Object);

            TraceManager.ClearTracers();

            Assert.AreEqual(0, TraceManager.Tracers.Count);
        }

        [Test]
        public void ExceptionsShouldBeLogged()
        {
            const string errorMessage = "Something bad happened";

            var tracerThatThrow = new Mock<ITracer>();
            tracerThatThrow.Setup(tracer1 => tracer1.Record(It.IsAny<Record>())).Throws(new Exception(errorMessage));
            TraceManager.RegisterTracer(tracerThatThrow.Object);

            var record = new Record(new SpanState(0, null, 1, isSampled: null, isDebug: false), TimeUtils.UtcNow, Annotations.ClientRecv());

            Assert.DoesNotThrow(() => TraceManager.Push(record));

            _mockLogger.Verify(logger1 => logger1.LogWarning(It.Is<string>(s => s.Contains(errorMessage))), Times.Once());
        }

        [Test]
        public void ExceptionsShouldnotPreventRecordToOtherTracers()
        {
            var tracerThatThrows = new Mock<ITracer>();
            tracerThatThrows.Setup(tracer1 => tracer1.Record(It.IsAny<Record>())).Throws(new Exception());
            TraceManager.RegisterTracer(tracerThatThrows.Object);


            var tracerThatDontThrow = new Mock<ITracer>();
            TraceManager.RegisterTracer(tracerThatDontThrow.Object); // on the assumption that tracer registration order is preserved

            var record = new Record(new SpanState(0, null, 1, isSampled: null, isDebug: false), TimeUtils.UtcNow, Annotations.ClientRecv());
            TraceManager.Push(record);

            tracerThatThrows.Verify(t => t.Record(It.IsAny<Record>()));
            tracerThatDontThrow.Verify(t => t.Record(It.IsAny<Record>()));
        }

        [Test]
        public void CannotStartMultipleTimes()
        {
            Assert.True(TraceManager.Started, "Test setup failed?");
            Assert.False(TraceManager.Start(_mockLogger.Object));
        }
    }
}
