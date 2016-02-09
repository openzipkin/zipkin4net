using System;
using Criteo.Profiling.Tracing.Sampling;
using Criteo.Profiling.Tracing.Tracers;
using Moq;
using NUnit.Framework;

namespace Criteo.Profiling.Tracing.UTest.Tracers
{
    [TestFixture]
    class T_SampleTracer
    {
        private Mock<ITracer> _mockUnderlyingTracer;

        [SetUp]
        public void SetUp()
        {
            _mockUnderlyingTracer = new Mock<ITracer>();
        }

        [Test]
        public void FlagSampledShouldForward()
        {
            var sampleTracer = new SamplerTracer(_mockUnderlyingTracer.Object);

            RecordTrace(sampleTracer, Flags.Empty().SetSampled());

            _mockUnderlyingTracer.Verify(tracer => tracer.Record(It.IsAny<Record>()), Times.Once());
        }

        [Test]
        public void FlagNotSampledShouldNotForward()
        {
            var sampleTracer = new SamplerTracer(_mockUnderlyingTracer.Object);

            RecordTrace(sampleTracer, Flags.Empty().SetNotSampled());

            _mockUnderlyingTracer.Verify(tracer => tracer.Record(It.IsAny<Record>()), Times.Never());
        }

        [Test]
        public void FlagUnsetShouldForwardForBackwardCompatibility()
        {
            var sampleTracer = new SamplerTracer(_mockUnderlyingTracer.Object);

            RecordTrace(sampleTracer, Flags.Empty());

            _mockUnderlyingTracer.Verify(tracer => tracer.Record(It.IsAny<Record>()), Times.Once());
        }

        private static void RecordTrace(ITracer tracer, Flags flags)
        {
            var spanId = new SpanId(1, 0, 1, flags);
            var record = new Record(spanId, DateTime.UtcNow, Annotations.ClientRecv());

            tracer.Record(record);
        }

    }
}
