using System;
using System.Linq;
using Moq;
using NUnit.Framework;

namespace Criteo.Profiling.Tracing.UTest
{
    [TestFixture]
    class T_Tracer
    {
        private Mock<ILogger> _mockLogger;
        private Mock<ITracer> _mockTracer;

        [SetUp]
        public void Setup()
        {
            Tracer.Clear();

            _mockLogger = new Mock<ILogger>();
            Trace.Configuration = new Configuration { Logger = _mockLogger.Object };

            _mockTracer = new Mock<ITracer>();
            Tracer.Register(_mockTracer.Object);
        }

        [Test]
        public void TracersAreCorrectlyRegistered()
        {
            Assert.AreEqual(1, Tracer.Tracers.Count);
            Assert.AreEqual(_mockTracer.Object, Tracer.Tracers.First());

            var secondMockTracer = new Mock<ITracer>();

            Tracer.Register(secondMockTracer.Object);

            Assert.AreEqual(2, Tracer.Tracers.Count);
            Assert.True(Tracer.Tracers.Contains(_mockTracer.Object));
            Assert.True(Tracer.Tracers.Contains(secondMockTracer.Object));
        }

        [Test]
        public void TracersAreIndeedCleared()
        {
            var secondTracer = new Mock<ITracer>();
            Tracer.Register(secondTracer.Object);

            Assert.AreEqual(2, Tracer.Tracers.Count);

            Tracer.Clear();

            Assert.AreEqual(0, Tracer.Tracers.Count);
        }

        [Test]
        public void ExceptionsAreCatchedWhileRecording()
        {
            const string errorMsg = "Something bad happened somewhere";

            _mockTracer.Setup(tracer1 => tracer1.Record(It.IsAny<Record>())).Throws(new Exception(errorMsg));

            var record = new Record(new SpanId(0, null, 1, Flags.Empty()), DateTime.UtcNow, Annotations.ClientRecv());

            Assert.DoesNotThrow(() => Tracer.Push(record));

            _mockLogger.Verify(logger1 => logger1.LogWarning(It.Is<string>(s => s.Contains(errorMsg))), Times.Once());
        }

    }
}
