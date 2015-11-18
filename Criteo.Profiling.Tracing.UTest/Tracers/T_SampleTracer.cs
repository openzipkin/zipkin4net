using System;
using Criteo.Profiling.Tracing.Tracers;
using Moq;
using NUnit.Framework;

namespace Criteo.Profiling.Tracing.UTest.Tracers
{
    [TestFixture]
    class T_SampleTracer
    {
        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ConstructorShouldThrowWithNegativeSamplingRate()
        {
            var mockTracer = new Mock<ITracer>();
            var sampleTracer = new SamplerTracer(underlyingTracer: mockTracer.Object, sampleRate: -0.1f);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ConstructorShouldThrowWithSuperiorToOneSamplingRate()
        {
            var mockTracer = new Mock<ITracer>();
            var sampleTracer = new SamplerTracer(underlyingTracer: mockTracer.Object, sampleRate: 1.1f);
        }

        [Test]
        public void SamplingRateSetterShouldThrow()
        {
            var mockTracer = new Mock<ITracer>();
            var sampleTracer = new SamplerTracer(underlyingTracer: mockTracer.Object, sampleRate: 1.0f);

            Assert.Throws<ArgumentOutOfRangeException>(() => sampleTracer.SampleRate = 2);
            Assert.Throws<ArgumentOutOfRangeException>(() => sampleTracer.SampleRate = -2);
        }

        [Test]
        public void FlagSampledTrueShouldAlwaysTrace()
        {
            var underlyingTracer = new Mock<ITracer>();
            var sampleTracer = new SamplerTracer(underlyingTracer.Object, sampleRate: 0f);

            var spanId = new SpanId(1, 0, 1, Flags.Empty().SetSampled());
            var record = new Record(spanId, DateTime.UtcNow, Annotations.ClientRecv());

            sampleTracer.Record(record);

            underlyingTracer.Verify(tracer => tracer.Record(It.IsAny<Record>()), Times.Once());
        }

        [Test]
        public void FlagSampledFalseShouldNeverTrace()
        {
            var underlyingTracer = new Mock<ITracer>();
            var sampleTracer = new SamplerTracer(underlyingTracer.Object, sampleRate: 1f);

            var spanId = new SpanId(1, 0, 1, Flags.Empty().SetNotSampled());
            var record = new Record(spanId, DateTime.UtcNow, Annotations.ClientRecv());

            sampleTracer.Record(record);

            underlyingTracer.Verify(tracer => tracer.Record(It.IsAny<Record>()), Times.Never());
        }

    }
}
