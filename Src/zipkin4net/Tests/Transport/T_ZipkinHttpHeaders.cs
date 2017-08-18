using zipkin4net.Transport;
using NUnit.Framework;

namespace zipkin4net.UTest.Transport
{
    [TestFixture]
    internal class T_ZipkinHttpHeaders
    {
        [TestCase("null", SpanFlags.None)]
        [TestCase("", SpanFlags.None)]
        [TestCase("notANumber", SpanFlags.None)]
        [TestCase("2notANumber", SpanFlags.None)]
        [TestCase("0", SpanFlags.None)]
        [TestCase("1", SpanFlags.Debug)]
        [TestCase("2", SpanFlags.SamplingKnown)]
        [TestCase("3", SpanFlags.Debug | SpanFlags.SamplingKnown)]
        [TestCase("6", SpanFlags.SamplingKnown | SpanFlags.Sampled)]
        public void FlagsAreCorrectlyParsed(string flagStr, SpanFlags expectedFlags)
        {
            var parsedFlags = ZipkinHttpHeaders.ParseFlagsHeader(flagStr);

            Assert.AreEqual(expectedFlags, parsedFlags);
        }

        [TestCase("null", null)]
        [TestCase("", null)]
        [TestCase("notABoolean", null)]
        [TestCase("0", false)]
        [TestCase("false", false)]
        [TestCase("FalSE", false)]
        [TestCase("1", true)]
        [TestCase("true", true)]
        [TestCase("TrUE", true)]
        public void SampledIsCorrectlyParsed(string sampledStr, bool? expectedSampledValue)
        {
            var sampledValue = ZipkinHttpHeaders.ParseSampledHeader(sampledStr);

            Assert.AreEqual(expectedSampledValue, sampledValue);
        }
    }
}
