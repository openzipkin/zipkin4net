using Criteo.Profiling.Tracing.Transport;
using NUnit.Framework;

namespace Criteo.Profiling.Tracing.UTest.Transport
{
    [TestFixture]
    internal class T_ZipkinHttpHeaders
    {
        [Test]
        public void LongIdEncodingIsCorrect()
        {
            const long notEncodedLong = 170;
            const string expectedEncodedLong = "00000000000000AA";

            var encodedLong = ZipkinHttpHeaders.EncodeLongToHexString(notEncodedLong);

            Assert.AreEqual(expectedEncodedLong, encodedLong);
        }

        [Test]
        public void LongIdDecodingIsCorrect()
        {
            const string encodedLong = "00000000000000AA";
            const long expectedLong = 170;

            var decodedLong = ZipkinHttpHeaders.DecodeHexString(encodedLong);
            Assert.AreEqual(expectedLong, decodedLong);
        }

        [Test]
        [Description("Check that encode is the inverse function of decode. In other words that x = encode(decode(x)) and y = decode(encode(y))")]
        public void IdEncodingDecodingGivesOriginalValues()
        {
            const long input = 10;

            var encoded = ZipkinHttpHeaders.EncodeLongToHexString(input);
            var decoded = ZipkinHttpHeaders.DecodeHexString(encoded);

            Assert.AreEqual(input, decoded);

            const string encodedInput = "00000000000000AA";
            var decodedInput = ZipkinHttpHeaders.DecodeHexString(encodedInput);
            var reEncodedInput = ZipkinHttpHeaders.EncodeLongToHexString(decodedInput);

            Assert.AreEqual(encodedInput, reEncodedInput);
        }

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
