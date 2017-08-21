using zipkin4net.Utils;
using NUnit.Framework;

namespace zipkin4net.UTest.Utils
{
    [TestFixture]
    internal class T_NumberUtils
    {
        [Test]
        public void TransformationIsReversible()
        {
            const long longId = 150L;

            var guid = NumberUtils.LongToGuid(longId);
            var backToLongId = NumberUtils.GuidToLong(guid);

            Assert.AreEqual(longId, backToLongId);
        }

        [Test]
        public void LongIdLowerEncodingIsCorrect()
        {
            const long notEncodedLong = 170;
            const string expectedEncodedLong = "00000000000000aa";

            var encodedLong = NumberUtils.EncodeLongToLowerHexString(notEncodedLong);

            Assert.AreEqual(expectedEncodedLong, encodedLong);
        }

        [Test]
        public void LongIdDecodingIsCorrect()
        {
            const string encodedLong = "00000000000000AA";
            const long expectedLong = 170;

            var decodedLong = NumberUtils.DecodeHexString(encodedLong);
            Assert.AreEqual(expectedLong, decodedLong);
        }

        [Test]
        [Description("Check that encode is the inverse function of decode. In other words that x = encode(decode(x)) and y = decode(encode(y))")]
        public void IdEncodingDecodingGivesOriginalValues()
        {
            const long input = 10;

            var encoded = NumberUtils.EncodeLongToLowerHexString(input);
            var decoded = NumberUtils.DecodeHexString(encoded);

            Assert.AreEqual(input, decoded);

            const string encodedInput = "00000000000000aa";
            var decodedInput = NumberUtils.DecodeHexString(encodedInput);
            var reEncodedInput = NumberUtils.EncodeLongToLowerHexString(decodedInput);

            Assert.AreEqual(encodedInput, reEncodedInput);
        }
    }
}
