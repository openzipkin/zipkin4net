using zipkin4net.Tracers.Zipkin;
using NUnit.Framework;

namespace zipkin4net.UTest.Tracers.Zipkin
{
    [TestFixture]
    internal class T_BinaryAnnotationValueEncoder
    {

        [Test]
        public void StringIsEncodedAsUtf8()
        {
            var bytes = BinaryAnnotationValueEncoder.Encode("$Helloé");
            var expected = new byte[] { 0x24, 0x48, 0x65, 0x6C, 0x6C, 0x6F, 0xC3, 0xA9 };
            Assert.AreEqual(expected, bytes);
        }

        [Test]
        public void ShortIsCorrectlyEncoded()
        {
            var bytes = BinaryAnnotationValueEncoder.Encode((short)1025);
            var expected = new byte[] { 0x04, 0x01 };
            Assert.AreEqual(expected, bytes);
        }

        [Test]
        public void IntIsCorrectlyEncoded()
        {
            var bytes = BinaryAnnotationValueEncoder.Encode((int)133124);
            var expected = new byte[] { 0x00, 0x02, 0x08, 0x04 };
            Assert.AreEqual(expected, bytes);
        }

        [Test]
        public void LongIsCorrectlyEncoded()
        {
            var bytes = BinaryAnnotationValueEncoder.Encode((long)1099511892096);
            var expected = new byte[] { 0x00, 0x00, 0x01, 0x00, 0x00, 0x04, 0x08, 0x80 };
            Assert.AreEqual(expected, bytes);
        }

        [Test]
        public void BoolIsCorrectlyEncoded()
        {
            Assert.AreEqual(new byte[] { 0x01 }, BinaryAnnotationValueEncoder.Encode(true));
            Assert.AreEqual(new byte[] { 0x00 }, BinaryAnnotationValueEncoder.Encode(false));
        }


        [Test]
        public void DoubleIsCorrectlyEncoded()
        {
            var bytes = BinaryAnnotationValueEncoder.Encode((double)12.5);
            var expected = new byte[] { 0x40, 0x29, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            Assert.AreEqual(expected, bytes);
        }


    }
}
