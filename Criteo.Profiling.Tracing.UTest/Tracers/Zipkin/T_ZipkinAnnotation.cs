using System;
using Criteo.Profiling.Tracing.Tracers.Zipkin;
using NUnit.Framework;

namespace Criteo.Profiling.Tracing.UTest.Tracers.Zipkin
{
    [TestFixture]
    class T_ZipkinAnnotation
    {
        [Test]
        public void TimestampGenerationIsCorrect()
        {
            // from timestampgenerator.com, expressed in microseconds
            const long expectedTimestamp = 636981516000000;

            var utcDateTime = new DateTime(1990, 3, 9, 11, 18, 36, DateTimeKind.Utc);

            var timestamp = ZipkinAnnotation.ToUnixTimestamp(utcDateTime);

            Assert.AreEqual(expectedTimestamp, timestamp);
        }

        [Test]
        public void ThriftConversionIsCorrect()
        {
            var now = DateTime.UtcNow;
            const string value = "anything";
            var ann = new ZipkinAnnotation(now, value, 0);

            var thriftAnn = ann.ToThrift();

            Assert.NotNull(thriftAnn);
            Assert.AreEqual(ZipkinAnnotation.ToUnixTimestamp(now), thriftAnn.Timestamp);
            Assert.AreEqual(value, thriftAnn.Value);
            Assert.IsNull(thriftAnn.Host);
            Assert.IsNull(thriftAnn.Duration);
        }

        [Test]
        public void ThriftConversionWithDurationIsCorrect()
        {
            var now = DateTime.UtcNow;
            const string value = "anything";

            var ann = new ZipkinAnnotation(now, value, 10250);
            var thriftAnn = ann.ToThrift();

            Assert.NotNull(thriftAnn);
            Assert.AreEqual(ZipkinAnnotation.ToUnixTimestamp(now), thriftAnn.Timestamp);
            Assert.AreEqual(value, thriftAnn.Value);
            Assert.IsNull(thriftAnn.Host);
            Assert.AreEqual(10250, thriftAnn.Duration);
        }

        [Test]
        public void ThriftConversionWithDurationOverflowIsCorrect()
        {
            var now = DateTime.UtcNow;
            const string value = "anything";

            var ann = new ZipkinAnnotation(now, value, int.MaxValue + 250L);
            var thriftAnn = ann.ToThrift();

            Assert.NotNull(thriftAnn);
            Assert.AreEqual(ZipkinAnnotation.ToUnixTimestamp(now), thriftAnn.Timestamp);
            Assert.AreEqual(value, thriftAnn.Value);
            Assert.IsNull(thriftAnn.Host);
            Assert.AreEqual(int.MaxValue, thriftAnn.Duration);
        }

    }
}
