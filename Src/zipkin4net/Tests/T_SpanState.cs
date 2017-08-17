using NUnit.Framework;

namespace Criteo.Profiling.Tracing.UTest
{
    [TestFixture]
    public class T_SpanState
    {
        private const long traceIdHigh = SpanState.NoTraceIdHigh;
        private const long traceId = 1;
        private const long spanId = 1;
        private const SpanFlags flags = SpanFlags.None;

        [Test]
        public void HashCodeShouldVaryIfTraceIdsAreNotEqual()
        {
            var spanState1 = new SpanState(traceIdHigh, 1, null, spanId, flags);
            var spanState2 = new SpanState(traceIdHigh, 2, null, spanId, flags);
            Assert.AreNotEqual(spanState1.GetHashCode(), spanState2.GetHashCode());
        }

        [Test]
        public void EqualsShouldReturnFalseIfTraceIdsAreNotEqual()
        {
            var spanState1 = new SpanState(traceIdHigh, 1, null, spanId, flags);
            var spanState2 = new SpanState(traceIdHigh, 2, null, spanId, flags);
            Assert.AreNotEqual(spanState1, spanState2);
        }

        [Test]
        public void HashCodeShouldVaryIfTraceIdHighsAreNotEqual()
        {
            var spanState1 = new SpanState(1, traceId, null, spanId, flags);
            var spanState2 = new SpanState(2, traceId, null, spanId, flags);
            Assert.AreNotEqual(spanState1.GetHashCode(), spanState2.GetHashCode());
        }

        [Test]
        public void EqualsShouldReturnFalseIfTraceIdHighsAreNotEqual()
        {
            var spanState1 = new SpanState(1, traceId, null, spanId, flags);
            var spanState2 = new SpanState(2, traceId, null, spanId, flags);
            Assert.AreNotEqual(spanState1, spanState2);
        }

        [Test]
        public void HashCodeShouldVaryIfParentSpanIdsAreNotEqual()
        {
            var spanState1 = new SpanState(traceIdHigh, traceId, 1, spanId, flags);
            var spanState2 = new SpanState(traceIdHigh, traceId, 2, spanId, flags);
            Assert.AreNotEqual(spanState1.GetHashCode(), spanState2.GetHashCode());
        }

        [Test]
        public void EqualsShouldReturnFalseIfParentSpanIdsAreNotEqual()
        {
            var spanState1 = new SpanState(traceIdHigh, traceId, 1, spanId, flags);
            var spanState2 = new SpanState(traceIdHigh, traceId, 2, spanId, flags);
            Assert.AreNotEqual(spanState1, spanState2);
        }

        [Test]
        public void HashCodeShouldVaryIfSpanIdsAreNotEqual()
        {
            var spanState1 = new SpanState(traceIdHigh, traceId, null, 1, flags);
            var spanState2 = new SpanState(traceIdHigh, traceId, null, 2, flags);
            Assert.AreNotEqual(spanState1.GetHashCode(), spanState2.GetHashCode());
        }

        [Test]
        public void EqualsShouldReturnFalseIfSpanIdsAreNotEqual()
        {
            var spanState1 = new SpanState(traceIdHigh, traceId, null, 1, flags);
            var spanState2 = new SpanState(traceIdHigh, traceId, null, 2, flags);
            Assert.AreNotEqual(spanState1, spanState2);
        }

        [Test]
        public void TraceIdHighDefaultToZero()
        {
            var spanState = new SpanState(traceId, null, spanId, flags);
            Assert.AreEqual(SpanState.NoTraceIdHigh, spanState.TraceIdHigh);
        }
    }
}
