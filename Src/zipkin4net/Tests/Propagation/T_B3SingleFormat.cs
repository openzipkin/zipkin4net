using NUnit.Framework;
using zipkin4net.Propagation;

namespace zipkin4net.UTest.Propagation
{
    [TestFixture]
    public class B3SingleFormatTest
    {
        private const string TraceId = "0000000000000001";
        private const string ParentId = "0000000000000002";
        private const string SpanId = "0000000000000003";

        [Test]
        public void WriteB3SingleFormat_notYetSampled()
        {
            ITraceContext context =
                new SpanState(traceId: 1, parentSpanId: null, spanId: 3, isSampled: null, isDebug: false);

            var b3Format = B3SingleFormat.WriteB3SingleFormat(context);
            Assert.AreEqual(TraceId + '-' + SpanId, b3Format);
        }

        [Test]
        public void writeB3SingleFormat_notYetSampled_128()
        {
            ITraceContext context = new SpanState(traceIdHigh: 9, traceId: 1, parentSpanId: null, spanId: 3,
                isSampled: null, isDebug: false);

            var b3Format = B3SingleFormat.WriteB3SingleFormat(context);
            Assert.AreEqual("0000000000000009" + TraceId + "-" + SpanId, b3Format);
        }

        [Test]
        public void writeB3SingleFormat_unsampled()
        {
            ITraceContext context = new SpanState(traceId: 1, parentSpanId: null, spanId: 3, isSampled: false,
                isDebug: false);
            var b3Format = B3SingleFormat.WriteB3SingleFormat(context);
            Assert.AreEqual(TraceId + '-' + SpanId + "-0", b3Format);
        }

        [Test]
        public void writeB3SingleFormat_sampled()
        {
            ITraceContext context =
                new SpanState(traceId: 1, parentSpanId: null, spanId: 3, isSampled: true, isDebug: false);
            var b3Format = B3SingleFormat.WriteB3SingleFormat(context);
            Assert.AreEqual(TraceId + '-' + SpanId + "-1", b3Format);
        }

        [Test]
        public void writeB3SingleFormat_debug()
        {
            ITraceContext context =
                new SpanState(traceId: 1, parentSpanId: null, spanId: 3, isSampled: null, isDebug: true);

            var b3Format = B3SingleFormat.WriteB3SingleFormat(context);
            Assert.AreEqual(TraceId + '-' + SpanId + "-d", b3Format);
        }

        [Test]
        public void writeB3SingleFormat_parent()
        {
            ITraceContext context =
                new SpanState(traceId: 1, parentSpanId: 2, spanId: 3, isSampled: true, isDebug: false);

            var b3Format = B3SingleFormat.WriteB3SingleFormat(context);
            Assert.AreEqual(TraceId + '-' + SpanId + "-1-" + ParentId, b3Format);
        }

        [Test]
        public void writeB3SingleFormatWithoutParent_unsampled()
        {
            ITraceContext context =
                new SpanState(traceId: 1, parentSpanId: 2, spanId: 3, isSampled: false, isDebug: false);

            var b3Format = B3SingleFormat.WriteB3SingleFormatWithoutParentId(context);
            Assert.AreEqual(TraceId + '-' + SpanId + "-0", b3Format);
        }

        [Test]
        public void writeB3SingleFormatWithoutParent_sampled()
        {
            ITraceContext context =
                new SpanState(traceId: 1, parentSpanId: 2, spanId: 3, isSampled: true, isDebug: false);

            var b3Format = B3SingleFormat.WriteB3SingleFormatWithoutParentId(context);
            Assert.AreEqual(TraceId + '-' + SpanId + "-1", b3Format);
        }

        [Test]
        public void writeB3SingleFormatWithoutParent_debug()
        {
            ITraceContext context =
                new SpanState(traceId: 1, parentSpanId: 2, spanId: 3, isSampled: false, isDebug: true);

            var b3Format = B3SingleFormat.WriteB3SingleFormatWithoutParentId(context);
            Assert.AreEqual(TraceId + '-' + SpanId + "-d", b3Format);
        }

        [Test]
        public void parseB3SingleFormat_middleOfString()
        {
            const string input = TraceId + TraceId + "-" + SpanId;
            var context = B3SingleFormat.ParseB3SingleFormat(input);
            var expectedContext = new SpanState(1, 1, null, 3, null, false);
            Assert.AreEqual(expectedContext, context);
        }

        
        [Test]
        public void parseB3SingleFormat_idsNotYetSampled()
        {
            const string input = TraceId + "-" + SpanId;
            var context = B3SingleFormat.ParseB3SingleFormat(input);
            var expectedContext = new SpanState(1, null, 3, null, false);
            Assert.AreEqual(expectedContext, context);
        }

        [Test]
        public void parseB3SingleFormat_idsUnsampled()
        {
            const string input = TraceId + "-" + SpanId + "-0";
            var context = B3SingleFormat.ParseB3SingleFormat(input);
            var expectedContext = new SpanState(1, null, 3, false, false);
            Assert.AreEqual(expectedContext, context);
        }

        [Test]
        public void parseB3SingleFormat_parent_unsampled()
        {
            const string input = TraceId + "-" + SpanId + "-0-" + ParentId;
            var context = B3SingleFormat.ParseB3SingleFormat(input);
            var expectedContext = new SpanState(1, 2, 3, false, false);
            Assert.AreEqual(expectedContext, context);
            }

        [Test]
        public void parseB3SingleFormat_parent_debug()
        {
            const string input = TraceId + "-" + SpanId + "-d-" + ParentId;
            var context = B3SingleFormat.ParseB3SingleFormat(input);
            var expectedContext = new SpanState(1, 2, 3, null, true);
            Assert.AreEqual(expectedContext, context);
        }

        [Test]
        public void parseB3SingleFormat_idsWithDebug()
        {
            const string input = TraceId + "-" + SpanId + "-d";
            var context = B3SingleFormat.ParseB3SingleFormat(input);
            var expectedContext = new SpanState(1, null, 3, null, true);
            Assert.AreEqual(expectedContext, context);
        }

        [Test]
        public void parseB3SingleFormat_malformed_traceId()
        {
            var input = TraceId.Substring(0, 15) + "?-" + SpanId;
            Assert.IsNull(B3SingleFormat.ParseB3SingleFormat(input)); // instead of raising exception
        }

        [Test]
        public void parseB3SingleFormat_malformed_id()
        {
            var input = TraceId + "-" + SpanId.Substring(0, 15) + "?";
            Assert.IsNull(B3SingleFormat.ParseB3SingleFormat(input)); // instead of raising exception
        }

        [Test]
        public void parseB3SingleFormat_malformed_sampled_parentid()
        {
            var input = TraceId + "-" + SpanId + "-1-" + ParentId.Substring(0, 15) + "?";
            Assert.IsNull(B3SingleFormat.ParseB3SingleFormat(input)); // instead of raising exception
        }

        // odd but possible to not yet sample a child
        [Test]
        public void parseB3SingleFormat_malformed_parentid_notYetSampled()
        {
            var input = TraceId + "-" + SpanId + "-" + ParentId.Substring(0, 15) + "?";
            Assert.IsNull(B3SingleFormat.ParseB3SingleFormat(input));
        }

        [Test]
        public void parseB3SingleFormat_malformed()
        {
            const string input = "not-a-tumor";
            Assert.IsNull(B3SingleFormat.ParseB3SingleFormat(input));
        }

        [Test]
        public void parseB3SingleFormat_malformed_uuid()
        {
            const string input = "b970dafd-0d95-40aa-95d8-1d8725aebe40";
            Assert.IsNull(B3SingleFormat.ParseB3SingleFormat(input));
        }

        [Test]
        public void parseB3SingleFormat_truncated()
        {
            Assert.IsNull(B3SingleFormat.ParseB3SingleFormat(""));
            Assert.IsNull(B3SingleFormat.ParseB3SingleFormat("-"));
            Assert.IsNull(B3SingleFormat.ParseB3SingleFormat("-1"));
            Assert.IsNull(B3SingleFormat.ParseB3SingleFormat("1-"));


            Assert.IsNull(B3SingleFormat.ParseB3SingleFormat(TraceId.Substring(0, 15)));
            Assert.IsNull(B3SingleFormat.ParseB3SingleFormat(TraceId));
            Assert.IsNull(B3SingleFormat.ParseB3SingleFormat(TraceId + "-"));
            Assert.IsNull(B3SingleFormat.ParseB3SingleFormat(TraceId.Substring(0, 15) + "-" + SpanId));
            Assert.IsNull(B3SingleFormat.ParseB3SingleFormat(TraceId + "-" + SpanId.Substring(0, 15)));
            Assert.IsNull(B3SingleFormat.ParseB3SingleFormat(TraceId + "-" + SpanId + "-"));
            Assert.IsNull(B3SingleFormat.ParseB3SingleFormat(TraceId + "-" + SpanId + "-1-"));
            Assert.IsNull(
                B3SingleFormat.ParseB3SingleFormat(TraceId + "-" + SpanId + "-1-" + ParentId.Substring(0, 15)));
        }

        [Test]
        public void parseB3SingleFormat_tooBig()
        {
            // overall length is ok, but it is malformed as parent is too long
            Assert.IsNull(B3SingleFormat.ParseB3SingleFormat(TraceId + "-" + SpanId + "-" + TraceId + TraceId));
            // overall length is not ok
            Assert.IsNull(
                B3SingleFormat.ParseB3SingleFormat(TraceId + TraceId + TraceId + "-" + SpanId + "-" + TraceId));
        }
    }
}