using System.Collections.Generic;
using System.Collections.Specialized;
using Criteo.Profiling.Tracing.Transport;
using NUnit.Framework;

namespace Criteo.Profiling.Tracing.UTest.Transport
{
    [TestFixture]
    class T_HttpTraceContext
    {

        [Test]
        [TestCase("", "", "", "", "")]
        [TestCase(null, null, null, null, null)]
        [TestCase("0000000000000001", null, null, null, null)]
        [TestCase("0000000000000001", "", null, null, null)]
        [TestCase("0000000000000001", "0000000000000000", null, null, null)]
        [TestCase("0000000000000001", "0000000000000000", "", null, null)]
        public void FailsToParseTraceFromNullOrEmpty(string encodedTraceId, string encodedSpanId, string encodedParentSpanId, string sampledStr, string flagsStr)
        {
            Trace receivedTrace;
            Assert.False(HttpTraceContext.TryParseTrace(encodedTraceId, encodedSpanId, encodedParentSpanId, sampledStr, flagsStr, out receivedTrace));
            Assert.IsNull(receivedTrace);
        }

        [Test]
        public void FailsToGetTraceFromEmptyHeaders()
        {
            Trace receivedTrace;

            // NameValueCollection
            Assert.False(HttpTraceContext.TryGet(new NameValueCollection(), out receivedTrace));
            Assert.IsNull(receivedTrace);

            // IDictionnary
            Assert.False(HttpTraceContext.TryGet(new Dictionary<string, string>(), out receivedTrace));
            Assert.IsNull(receivedTrace);
        }

        [Test]
        public void SerializedTraceIsEqualToOriginal()
        {
            Trace.SamplingRate = 1f;
            Trace.TracingEnabled = true;
            var originalTrace = Trace.CreateIfSampled();

            var headersNvc = new NameValueCollection();
            var headersDict = new Dictionary<string, string>();

            HttpTraceContext.Set(headersNvc, originalTrace);
            HttpTraceContext.Set(headersDict, originalTrace);

            Trace receivedTraceNvc;
            Assert.True(HttpTraceContext.TryGet(headersNvc, out receivedTraceNvc));
            Assert.AreEqual(originalTrace, receivedTraceNvc);

            Trace receivedTraceDict;
            Assert.True(HttpTraceContext.TryGet(headersDict, out receivedTraceDict));
            Assert.AreEqual(originalTrace, receivedTraceDict);
        }

        [Test]
        public void DeserializedTraceIsEqualToOriginal()
        {
            const string encodedTraceId = "0000000000000001";
            const string encodedSpanId = "0000000000000000";
            const string encodedParentSpanId = "00000000000000FA";
            const string flagsStr = "0";
            const string sampledStr = null;


            Trace receivedTrace;
            Assert.True(HttpTraceContext.TryParseTrace(encodedTraceId, encodedSpanId, encodedParentSpanId, sampledStr, flagsStr, out receivedTrace));

            var recreatedHeaders = new NameValueCollection();
            HttpTraceContext.Set(recreatedHeaders, receivedTrace);

            Assert.AreEqual(4, recreatedHeaders.Count);

            Assert.AreEqual(encodedTraceId, recreatedHeaders[HttpTraceContext.TraceId]);
            Assert.AreEqual(encodedParentSpanId, recreatedHeaders[HttpTraceContext.ParentSpanId]);
            Assert.AreEqual(encodedSpanId, recreatedHeaders[HttpTraceContext.SpanId]);
            Assert.AreEqual(flagsStr, recreatedHeaders[HttpTraceContext.Flags]);
        }

        [Test]
        public void HeadersAreCorrectlySetNvc()
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
        public void HeadersAreCorrectlySetDict()
        {
            var traceId = new SpanId(1, 0, 250, Flags.Empty().SetSampled());
            var trace = Trace.CreateFromId(traceId);

            var headers = new Dictionary<string, string>();

            HttpTraceContext.Set(headers, trace);

            Assert.AreEqual(5, headers.Count);

            Assert.AreEqual("0000000000000001", headers[HttpTraceContext.TraceId]);
            Assert.AreEqual("0000000000000000", headers[HttpTraceContext.ParentSpanId]);
            Assert.AreEqual("00000000000000FA", headers[HttpTraceContext.SpanId]);
            Assert.AreEqual("6", headers[HttpTraceContext.Flags]);
            Assert.AreEqual("1", headers[HttpTraceContext.Sampled]);
        }

        [Test]
        public void ParseTraceWithoutFlags()
        {
            Trace trace;
            Assert.True(HttpTraceContext.TryParseTrace(encodedTraceId: "0000000000000001", encodedSpanId: "00000000000000FA", encodedParentSpanId: "0000000000000000", sampledStr: null, flagsStr: null, trace: out trace));

            Assert.AreEqual(1, trace.CurrentId.TraceId);
            Assert.AreEqual(0, trace.CurrentId.ParentSpanId);
            Assert.AreEqual(250, trace.CurrentId.Id);
            Assert.AreEqual(Flags.Empty(), trace.CurrentId.Flags);
        }

        [TestCase(null, null, null)]
        [TestCase("0000000000000001", null, null)]
        [TestCase(null, "00000000000000FA", null)]
        [TestCase(null, null, "0000000000000000")]
        [TestCase(null, "00000000000000FA", "0000000000000000")]
        [TestCase("0000000000000001", "00000000000000FA", null)]
        [TestCase("0000000000000001", null, "0000000000000000")]
        [Description("A missing required trace header should prevent the trace to be created (required headers are traceId, spanId and parentSpanId)")]
        public void ParseWithoutRequiredHeadersShouldFail(string encodedTraceId, string encodedSpanId, string encodedParentSpanId)
        {
            Trace trace;
            Assert.False(HttpTraceContext.TryParseTrace(encodedTraceId, encodedSpanId, encodedParentSpanId, sampledStr: "1", flagsStr: "0", trace: out trace));
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
