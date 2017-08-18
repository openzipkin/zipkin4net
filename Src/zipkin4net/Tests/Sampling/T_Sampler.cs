using System;
using zipkin4net.Sampling;
using NUnit.Framework;

namespace zipkin4net.UTest.Sampling
{
    [TestFixture]
    internal class T_Sampler
    {
        private const long NoSalt = 0;

        [TestCase(-0.001f)]
        [TestCase(1.001f)]
        public void InvalidRateShouldThrow(float rate)
        {
            var sampler = new DefaultSampler(NoSalt);
            Assert.Throws <ArgumentOutOfRangeException>(() => sampler.SamplingRate = rate);
        }

        [TestCase(0f)]
        [TestCase(0.5f)]
        [TestCase(1f)]
        public void ValidRateShouldntThrow(float rate)
        {
            var sampler = new DefaultSampler(NoSalt);
            Assert.DoesNotThrow(() => sampler.SamplingRate = rate);
        }

        [TestCase(1L)]
        [TestCase(0L)]
        [TestCase(-1L)]
        [TestCase(9999L)]
        [TestCase(10000L)]
        [TestCase(10001L)]
        public void RateZeroShouldntSampleTrue(long traceId)
        {
            var sampler = new DefaultSampler(NoSalt, 0f);
            Assert.False(sampler.Sample(traceId));
        }

        [TestCase(1L)]
        [TestCase(0L)]
        [TestCase(-1L)]
        [TestCase(9999L)]
        [TestCase(10000L)]
        [TestCase(10001L)]
        public void RateOneShouldAlwaysSampleTrue(long traceId)
        {
            var sampler = new DefaultSampler(NoSalt, 1f);
            Assert.True(sampler.Sample(traceId));
        }

        [Test]
        public void SampleWorksAsExpected()
        {
            var sampler = new DefaultSampler(NoSalt, 0.000005f);

            const int shouldTraceId = 4;
            Assert.True(sampler.Sample(shouldTraceId));

            const int notTraceId = 5;
            Assert.False(sampler.Sample(notTraceId));
        }

    }
}
