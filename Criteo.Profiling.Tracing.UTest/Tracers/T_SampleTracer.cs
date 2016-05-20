using Criteo.Profiling.Tracing.Tracers;
using Criteo.Profiling.Tracing.Utils;
using Moq;
using NUnit.Framework;

namespace Criteo.Profiling.Tracing.UTest.Tracers
{
    [TestFixture]
    internal class T_SampleTracer
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

            RecordTrace(sampleTracer, SpanFlags.SamplingKnown | SpanFlags.Sampled);

            _mockUnderlyingTracer.Verify(tracer => tracer.Record(It.IsAny<Record>()), Times.Once());
        }

        [Test]
        public void FlagNotSampledShouldNotForward()
        {
            var sampleTracer = new SamplerTracer(_mockUnderlyingTracer.Object);

            RecordTrace(sampleTracer, SpanFlags.SamplingKnown);

            _mockUnderlyingTracer.Verify(tracer => tracer.Record(It.IsAny<Record>()), Times.Never());
        }

        [Test]
        public void FlagUnsetShouldForwardForBackwardCompatibility()
        {
            var sampleTracer = new SamplerTracer(_mockUnderlyingTracer.Object);

            RecordTrace(sampleTracer, SpanFlags.None);

            _mockUnderlyingTracer.Verify(tracer => tracer.Record(It.IsAny<Record>()), Times.Once());
        }

        private static void RecordTrace(ITracer tracer, SpanFlags flags)
        {
            var spanState = new SpanState(1, 0, 1, flags);
            var record = new Record(spanState, TimeUtils.UtcNow, Annotations.ClientRecv());

            tracer.Record(record);
        }

    }
}
