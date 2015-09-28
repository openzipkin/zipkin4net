using System.Collections.Specialized;
using Criteo.Profiling.Tracing.Transport;
using NUnit.Framework;

namespace Criteo.Profiling.Tracing.UTest.Transport
{
    [TestFixture]
    class T_HttpTraceContext
    {

        [Test]
        public void FailsToGetTraceFromRequestWithoutContext()
        {
            var headers = new NameValueCollection();
            headers["Some-HTTP-header"] = "value";

            Trace receivedTrace;
            Assert.False(HttpTraceContext.TryGet(headers, out receivedTrace));
            Assert.IsNull(receivedTrace);
        }

        [Test]
        public void SerializedTraceIsEqualToOriginal()
        {
            var originalTrace = Trace.Create();

            var headers = new NameValueCollection();

            HttpTraceContext.Set(headers, originalTrace);

            Trace receivedTrace;
            Assert.True(HttpTraceContext.TryGet(headers, out receivedTrace));

            Assert.AreEqual(originalTrace, receivedTrace);
        }

        [Test]
        public void DeserializedTraceIsEqualToOriginal()
        {
            var headers = new NameValueCollection();
            headers[HttpTraceContext.TraceId] = "0000000000000001";
            headers[HttpTraceContext.ParentSpanId] = "0000000000000000";
            headers[HttpTraceContext.SpanId] = "00000000000000FA";
            headers[HttpTraceContext.Flags] = "0";

            Trace receivedTrace;
            Assert.True(HttpTraceContext.TryGet(headers, out receivedTrace));

            var recreatedHeaders = new NameValueCollection();
            HttpTraceContext.Set(recreatedHeaders, receivedTrace);

            Assert.AreEqual(headers.Count, recreatedHeaders.Count);

            Assert.AreEqual(headers[HttpTraceContext.TraceId], recreatedHeaders[HttpTraceContext.TraceId]);
            Assert.AreEqual(headers[HttpTraceContext.ParentSpanId], recreatedHeaders[HttpTraceContext.ParentSpanId]);
            Assert.AreEqual(headers[HttpTraceContext.SpanId], recreatedHeaders[HttpTraceContext.SpanId]);
            Assert.AreEqual(headers[HttpTraceContext.Flags], recreatedHeaders[HttpTraceContext.Flags]);
        }

        [Test]
        public void HeadersAreCorrectlySet()
        {
            var traceId = new SpanId(1, 0, 250, Flags.Empty().SetSampled());
            var trace = Trace.CreateFromId(traceId);

            var headers = new NameValueCollection();

            HttpTraceContext.Set(headers, trace);

            Assert.AreEqual(5, headers.Count);

            Assert.AreEqual("0000000000000001", headers[HttpTraceContext.TraceId]);
            Assert.AreEqual("0000000000000000", headers[HttpTraceContext.ParentSpanId]);
            Assert.AreEqual("00000000000000FA", headers[HttpTraceContext.SpanId]);
            Assert.AreEqual("6", headers[HttpTraceContext.Flags]);
            Assert.AreEqual("1", headers[HttpTraceContext.Sampled]);
        }

        [Test]
        public void GetWithoutFlagsIsCorrectlyCreated()
        {
            var headers = new NameValueCollection();
            headers[HttpTraceContext.TraceId] = "0000000000000001";
            headers[HttpTraceContext.ParentSpanId] = "0000000000000000";
            headers[HttpTraceContext.SpanId] = "00000000000000FA";

            Trace trace;
            Assert.True(HttpTraceContext.TryGet(headers, out trace));

            Assert.AreEqual(1, trace.CurrentId.TraceId);
            Assert.AreEqual(0, trace.CurrentId.ParentSpanId);
            Assert.AreEqual(250, trace.CurrentId.Id);
            Assert.AreEqual(Flags.Empty(), trace.CurrentId.Flags);
        }

        [Test]
        [Description("A missing required trace header should prevent the trace to be created (required headers are traceId, spanId and parentSpanId)")]
        public void GetWithoutRequiredHeadersShouldFail()
        {
            var headers = new NameValueCollection();
            headers[HttpTraceContext.Flags] = "0";
            headers[HttpTraceContext.Sampled] = "1";

            Trace trace;
            Assert.False(HttpTraceContext.TryGet(headers, out trace));

            headers[HttpTraceContext.TraceId] = "0000000000000001";
            headers[HttpTraceContext.SpanId] = null;
            headers[HttpTraceContext.ParentSpanId] = null;

            Assert.False(HttpTraceContext.TryGet(headers, out trace));

            headers[HttpTraceContext.TraceId] = null;
            headers[HttpTraceContext.SpanId] = "00000000000000FA";
            headers[HttpTraceContext.ParentSpanId] = null;

            Assert.False(HttpTraceContext.TryGet(headers, out trace));

            headers[HttpTraceContext.TraceId] = null;
            headers[HttpTraceContext.SpanId] = null;
            headers[HttpTraceContext.ParentSpanId] = "0000000000000000";

            Assert.False(HttpTraceContext.TryGet(headers, out trace));

            headers[HttpTraceContext.TraceId] = null;
            headers[HttpTraceContext.SpanId] = "00000000000000FA";
            headers[HttpTraceContext.ParentSpanId] = "0000000000000000";

            Assert.False(HttpTraceContext.TryGet(headers, out trace));

            headers[HttpTraceContext.TraceId] = "0000000000000001";
            headers[HttpTraceContext.SpanId] = "00000000000000FA";
            headers[HttpTraceContext.ParentSpanId] = null;

            Assert.False(HttpTraceContext.TryGet(headers, out trace));

            headers[HttpTraceContext.TraceId] = "0000000000000001";
            headers[HttpTraceContext.SpanId] = null;
            headers[HttpTraceContext.ParentSpanId] = "0000000000000000";

            Assert.False(HttpTraceContext.TryGet(headers, out trace));
        }

        [Test]
        public void LongIdEncodingIsCorrect()
        {
            const long notEncodedLong = 170;
            const string expectedEncodedLong = "00000000000000AA";

            var encodedLong = HttpTraceContext.EncodeLongToHexString(notEncodedLong);

            Assert.AreEqual(expectedEncodedLong, encodedLong);
        }

        [Test]
        public void LongIdDecodingIsCorrect()
        {
            const string encodedLong = "00000000000000AA";
            const long expectedLong = 170;

            var decodedLong = HttpTraceContext.DecodeHexString(encodedLong);
            Assert.AreEqual(expectedLong, decodedLong);
        }

        [Test]
        [Description("Check that encode is the inverse function of decode. In other words that x = encode(decode(x)) and y = decode(encode(y))")]
        public void IdEncodingDecodingGivesOriginalValues()
        {
            const long input = 10;

            var encoded = HttpTraceContext.EncodeLongToHexString(input);
            var decoded = HttpTraceContext.DecodeHexString(encoded);

            Assert.AreEqual(input, decoded);

            const string encodedInput = "00000000000000AA";
            var decodedInput = HttpTraceContext.DecodeHexString(encodedInput);
            var reEncodedInput = HttpTraceContext.EncodeLongToHexString(decodedInput);

            Assert.AreEqual(encodedInput, reEncodedInput);
        }

    }
}
