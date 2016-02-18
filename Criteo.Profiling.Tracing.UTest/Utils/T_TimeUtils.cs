using System;
using Criteo.Profiling.Tracing.Utils;
using NUnit.Framework;

namespace Criteo.Profiling.Tracing.UTest.Utils
{

    [TestFixture]
    class T_TimeUtils
    {
        [Test]
        public void TimestampGenerationIsCorrect()
        {
            // from timestampgenerator.com, expressed in microseconds
            const long expectedTimestamp = 636981516000000;

            var utcDateTime = new DateTime(1990, 3, 9, 11, 18, 36, DateTimeKind.Utc);

            var timestamp = TimeUtils.ToUnixTimestamp(utcDateTime);

            Assert.AreEqual(expectedTimestamp, timestamp);
        }
    }

}
