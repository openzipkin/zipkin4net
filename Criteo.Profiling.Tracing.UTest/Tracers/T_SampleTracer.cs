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

        private Mock<ISampler> _mockSampler;
        private Mock<ITracer> _mockUnderlyingTracer;

        [SetUp]
        public void SetUp()
        {
            _mockUnderlyingTracer = new Mock<ITracer>();
            _mockSampler = new Mock<ISampler>();
        }

        [Test]
        public void FlagSampledShouldByPassSamplingAndForward()
        {
            var sampleTracer = new SamplerTracer(_mockUnderlyingTracer.Object, _mockSampler.Object);

            RecordTrace(sampleTracer, Flags.Empty().SetSampled());

            _mockSampler.Verify(sampler1 => sampler1.Sample(It.IsAny<long>()), Times.Never());
            _mockUnderlyingTracer.Verify(tracer => tracer.Record(It.IsAny<Record>()), Times.Once());
        }

        [Test]
        public void FlagNotSampledShouldByPassSamplingAndNotForward()
        {
            var sampleTracer = new SamplerTracer(_mockUnderlyingTracer.Object, _mockSampler.Object);

            RecordTrace(sampleTracer, Flags.Empty().SetNotSampled());

            _mockSampler.Verify(sampler1 => sampler1.Sample(It.IsAny<long>()), Times.Never());
            _mockUnderlyingTracer.Verify(tracer => tracer.Record(It.IsAny<Record>()), Times.Never());
        }

        [TestCase(true)]
        public void FlagUnsetShouldRelyOnSampling(bool sampled)
        {
            var sampleTracer = new SamplerTracer(_mockUnderlyingTracer.Object, _mockSampler.Object);

            _mockSampler.Setup(sampler => sampler.Sample(It.IsAny<long>())).Returns(sampled);

            RecordTrace(sampleTracer, Flags.Empty());

            _mockSampler.Verify(sampler1 => sampler1.Sample(It.IsAny<long>()), Times.Once());
            _mockUnderlyingTracer.Verify(tracer => tracer.Record(It.IsAny<Record>()), sampled ? Times.Once() : Times.Never());
        }

        private static void RecordTrace(ITracer tracer, Flags flags)
        {
            var spanId = new SpanId(1, 0, 1, flags);
            var record = new Record(spanId, DateTime.UtcNow, Annotations.ClientRecv());

            tracer.Record(record);
        }

    }
}
