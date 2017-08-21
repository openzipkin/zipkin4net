using System;
using zipkin4net.Utils;
using NUnit.Framework;

namespace zipkin4net.UTest.Utils
{

    [TestFixture]
    internal class T_TimeUtils
    {
        [Test]
        public void TimestampGenerationIsCorrect()
        {
            // from timestampgenerator.com, expressed in microseconds
            const long expectedTimestamp = 636981516000000;

            var utcDateTime = new DateTime(1990, 3, 9, 11, 18, 36, DateTimeKind.Utc);

            var timestamp = utcDateTime.ToUnixTimestamp();

            Assert.AreEqual(expectedTimestamp, timestamp);
        }

        [Test]
        public void TimestampGenerationIsCorrectInLocal()
        {
            // from timestampgenerator.com, expressed in microseconds
            const long expectedTimestamp = 636981516000000;

            var utcDateTime = new DateTime(1990, 3, 9, 11, 18, 36, DateTimeKind.Local).Add(TimeZoneInfo.Local.BaseUtcOffset);

            var timestamp = utcDateTime.ToUnixTimestamp();

            Assert.AreEqual(expectedTimestamp, timestamp);
        }
    }

}
