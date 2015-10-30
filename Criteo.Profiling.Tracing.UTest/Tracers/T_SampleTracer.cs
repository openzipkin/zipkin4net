using System;
using System.Diagnostics;
using Criteo.Profiling.Tracing.Tracers;
using Moq;
using NUnit.Framework;

namespace Criteo.Profiling.Tracing.UTest.Tracers
{
    [TestFixture]
    class T_SampleTracer
    {

        [SetUp]
        public void SetUp()
        {
            Trace.TracingEnabled = true;
            Tracer.Clear();
        }

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
            var mockTracer = new Mock<ITracer>();
            var sampleTracer = new SamplerTracer(underlyingTracer: mockTracer.Object, sampleRate: 0f);

            Tracer.Register(sampleTracer);

            var trace = Trace.CreateFromId(new SpanId(1, 0, 1, Flags.Empty().SetSampled()));

            trace.Record(Annotations.ClientRecv()).Wait();

            mockTracer.Verify(tracer => tracer.Record(It.IsAny<Record>()), Times.Once());
        }

        [Test]
        public void FlagSampledFalseShouldNeverTrace()
        {
            var mockTracer = new Mock<ITracer>();

            var sampleTracer = new SamplerTracer(underlyingTracer: mockTracer.Object, sampleRate: 1f);

            Tracer.Register(sampleTracer);

            var trace = Trace.CreateFromId(new SpanId(1, 0, 1, Flags.Empty().SetNotSampled()));

            trace.Record(Annotations.ClientRecv()).Wait();

            mockTracer.Verify(tracer => tracer.Record(It.IsAny<Record>()), Times.Never());
        }

    }
}
