using NUnit.Framework;

namespace zipkin4net.UTest
{
    [TestFixture]
    public class T_SpanState
    {
        private const long TraceIdHigh = SpanState.NoTraceIdHigh;
        private const long TraceId = 1;
        private const long SpanId = 1;

        private static readonly bool? IsSampled = null;
        private const bool IsDebug = false;

        [Test]
        public void HashCodeShouldVaryIfTraceIdsAreNotEqual()
        {
            var spanState1 = new SpanState(TraceIdHigh, 1, null, SpanId, IsSampled, IsDebug);
            var spanState2 = new SpanState(TraceIdHigh, 2, null, SpanId, IsSampled, IsDebug);
            Assert.AreNotEqual(spanState1.GetHashCode(), spanState2.GetHashCode());
        }

        [Test]
        public void EqualsShouldReturnFalseIfTraceIdsAreNotEqual()
        {
            var spanState1 = new SpanState(TraceIdHigh, 1, null, SpanId, IsSampled, IsDebug);
            var spanState2 = new SpanState(TraceIdHigh, 2, null, SpanId, IsSampled, IsDebug);
            Assert.AreNotEqual(spanState1, spanState2);
        }

        [Test]
        public void HashCodeShouldVaryIfTraceIdHighsAreNotEqual()
        {
            var spanState1 = new SpanState(1, TraceId, null, SpanId, IsSampled, IsDebug);
            var spanState2 = new SpanState(2, TraceId, null, SpanId, IsSampled, IsDebug);
            Assert.AreNotEqual(spanState1.GetHashCode(), spanState2.GetHashCode());
        }

        [Test]
        public void EqualsShouldReturnFalseIfTraceIdHighsAreNotEqual()
        {
            var spanState1 = new SpanState(1, TraceId, null, SpanId, IsSampled, IsDebug);
            var spanState2 = new SpanState(2, TraceId, null, SpanId, IsSampled, IsDebug);
            Assert.AreNotEqual(spanState1, spanState2);
        }

        [Test]
        public void HashCodeShouldVaryIfParentSpanIdsAreNotEqual()
        {
            var spanState1 = new SpanState(TraceIdHigh, TraceId, 1, SpanId, IsSampled, IsDebug);
            var spanState2 = new SpanState(TraceIdHigh, TraceId, 2, SpanId, IsSampled, IsDebug);
            Assert.AreNotEqual(spanState1.GetHashCode(), spanState2.GetHashCode());
        }

        [Test]
        public void EqualsShouldReturnFalseIfParentSpanIdsAreNotEqual()
        {
            var spanState1 = new SpanState(TraceIdHigh, TraceId, 1, SpanId, IsSampled, IsDebug);
            var spanState2 = new SpanState(TraceIdHigh, TraceId, 2, SpanId, IsSampled, IsDebug);
            Assert.AreNotEqual(spanState1, spanState2);
        }

        [Test]
        public void HashCodeShouldVaryIfSpanIdsAreNotEqual()
        {
            var spanState1 = new SpanState(TraceIdHigh, TraceId, null, 1, IsSampled, IsDebug);
            var spanState2 = new SpanState(TraceIdHigh, TraceId, null, 2, IsSampled, IsDebug);
            Assert.AreNotEqual(spanState1.GetHashCode(), spanState2.GetHashCode());
        }

        [Test]
        public void EqualsShouldReturnFalseIfSpanIdsAreNotEqual()
        {
            var spanState1 = new SpanState(TraceIdHigh, TraceId, null, 1, IsSampled, IsDebug);
            var spanState2 = new SpanState(TraceIdHigh, TraceId, null, 2, IsSampled, IsDebug);
            Assert.AreNotEqual(spanState1, spanState2);
        }

        [Test]
        public void TraceIdHighDefaultToZero()
        {
            var spanState = new SpanState(TraceId, null, SpanId, IsSampled, IsDebug);
            Assert.AreEqual(SpanState.NoTraceIdHigh, spanState.TraceIdHigh);
        }
    }
}
