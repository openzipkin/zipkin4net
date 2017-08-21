using zipkin4net.Tracers.Zipkin;
using NUnit.Framework;

namespace zipkin4net.UTest.Tracers.Zipkin
{
    [TestFixture]
    internal class T_Statistics
    {

        [Test]
        public void InitialValueAreZeros()
        {
            var stats = new Statistics();

            Assert.AreEqual(0, stats.RecordProcessed);
            Assert.AreEqual(0, stats.SpanFlushed);
            Assert.AreEqual(0, stats.SpanSent);
            Assert.AreEqual(0, stats.SpanSentTotalBytes);
        }

        [Test]
        public void ValuesAreUpdated()
        {
            var stats = new Statistics();

            stats.UpdateRecordProcessed();
            Assert.AreEqual(1, stats.RecordProcessed);

            stats.UpdateSpanFlushed();
            Assert.AreEqual(1, stats.SpanFlushed);

            stats.UpdateSpanSent();
            Assert.AreEqual(1, stats.SpanSent);

            stats.UpdateSpanSentBytes(10);
            Assert.AreEqual(10, stats.SpanSentTotalBytes);
        }

    }
}
