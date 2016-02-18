using System;
using Criteo.Profiling.Tracing.Tracers.Zipkin;
using Criteo.Profiling.Tracing.Utils;
using NUnit.Framework;

namespace Criteo.Profiling.Tracing.UTest.Tracers.Zipkin
{
    [TestFixture]
    class T_ZipkinAnnotation
    {

        [Test]
        public void ThriftConversionIsCorrect()
        {
            var now = DateTime.UtcNow;
            const string value = "anything";
            var ann = new ZipkinAnnotation(now, value);

            var thriftAnn = ann.ToThrift();

            Assert.NotNull(thriftAnn);
            Assert.AreEqual(TimeUtils.ToUnixTimestamp(now), thriftAnn.Timestamp);
            Assert.AreEqual(value, thriftAnn.Value);
            Assert.IsNull(thriftAnn.Host);
            Assert.IsNull(thriftAnn.Duration);
        }

    }
}
