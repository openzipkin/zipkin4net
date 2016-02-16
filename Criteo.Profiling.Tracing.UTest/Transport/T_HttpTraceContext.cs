using System.Collections.Generic;
using System.Collections.Specialized;
using Criteo.Profiling.Tracing.Transport;
using Moq;
using NUnit.Framework;

namespace Criteo.Profiling.Tracing.UTest.Transport
{
    [TestFixture]
    class T_HttpTraceContext
    {

        private Mock<ILogger> _mockLogger;

        [SetUp]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger>();
            Trace.Logger = _mockLogger.Object;
        }

        #region TryGet/TryParse Context

        [Test]
        [Description("Required headers are traceId and spanId")]
        public void TryGetFailsWithoutRequiredHeaders()
        {
            Trace receivedTrace;

            // NameValueCollection
            Assert.False(HttpTraceContext.TryGet(new NameValueCollection(), out receivedTrace));
            Assert.IsNull(receivedTrace);

            // IDictionnary
            Assert.False(HttpTraceContext.TryGet(new Dictionary<string, string>(), out receivedTrace));
            Assert.IsNull(receivedTrace);

            Assert.False(HttpTraceContext.TryGet(new Dictionary<string, string> { { HttpTraceContext.TraceId, "0000000000000001" } }, out receivedTrace));
            Assert.IsNull(receivedTrace);

            Assert.False(HttpTraceContext.TryGet(new Dictionary<string, string> { { HttpTraceContext.SpanId, "0000000000000001" } }, out receivedTrace));
            Assert.IsNull(receivedTrace);
        }

        [TestCase(null, null)]
        [TestCase("", "")]
        [TestCase("0000000000000001", null)]
        [TestCase("0000000000000001", "")]
        [TestCase(null, "00000000000000FA")]
        [TestCase("", "00000000000000FA")]
        [Description("Required headers are traceId and spanId")]
        public void TryParseFailsWithoutRequiredHeaders(string encodedTraceId, string encodedSpanId)
        {
            Trace trace;
            Assert.False(HttpTraceContext.TryParseTrace(encodedTraceId, encodedSpanId, encodedParentSpanId: "0000000000000000", sampledStr: "1", flagsStr: "0", trace: out trace));
        }


        [TestCase("0000000000000000")]
        [TestCase(null)]
        public void ShouldParseTraceWithoutFlags(string encodedParentSpanId)
        {
            Trace trace;
            Assert.True(HttpTraceContext.TryParseTrace(encodedTraceId: "0000000000000001", encodedSpanId: "00000000000000FA", encodedParentSpanId: encodedParentSpanId, sampledStr: null, flagsStr: null, trace: out trace));

            Assert.AreEqual(1, trace.CurrentId.TraceId);
            Assert.AreEqual(250, trace.CurrentId.Id);
            Assert.AreEqual(Flags.Empty(), trace.CurrentId.Flags);

            var expectedParentSpanId = (encodedParentSpanId == null) ? (long?)null : 0L;
            Assert.AreEqual(expectedParentSpanId, trace.CurrentId.ParentSpanId);
        }

        #endregion

        #region Set Context

        [TestCase("0000000000000001", 0L, "0000000000000000", "00000000000000FA", true, "6", "1", 5)]
        [TestCase("0000000000000001", 0L, "0000000000000000", "00000000000000FA", false, "0", null, 4)]
        [TestCase("0000000000000001", null, null, "00000000000000FA", true, "6", "1", 4)]
        public void HeadersAreCorrectlySet(string expectedTraceId, long? parentSpanId, string expectedParentSpanId, string expectedSpanId, bool setSampled, string expectedFlags, string expectedSampled, int expectedCount)
        {
            var spanId = new SpanId(1, parentSpanId, 250, setSampled ? Flags.Empty().SetSampled() : Flags.Empty());
            var trace = Trace.CreateFromId(spanId);

            var headersNvc = new NameValueCollection();
            HttpTraceContext.Set(headersNvc, trace);
            CheckHeaders(headersNvc, expectedTraceId, expectedParentSpanId, expectedSpanId, expectedFlags, expectedSampled, expectedCount);

            var headersDict = new Dictionary<string, string>();
            HttpTraceContext.Set(headersDict, trace);
            CheckHeaders(headersDict, expectedTraceId, expectedParentSpanId, expectedSpanId, expectedFlags, expectedSampled, expectedCount);
        }

        private static void CheckHeaders(IReadOnlyDictionary<string, string> headers, string traceId, string parentSpanId, string spanId, string flags, string sampled, int count)
        {
            Assert.AreEqual(count, headers.Count);

            // Required fields
            Assert.AreEqual(traceId, headers[HttpTraceContext.TraceId]);
            Assert.AreEqual(spanId, headers[HttpTraceContext.SpanId]);
            Assert.AreEqual(flags, headers[HttpTraceContext.Flags]);

            // Optional fields
            if (parentSpanId != null)
            {
                Assert.AreEqual(parentSpanId, headers[HttpTraceContext.ParentSpanId]);
            }
            else
            {
                Assert.False(headers.ContainsKey(HttpTraceContext.ParentSpanId));
            }

            if (sampled != null)
            {
                Assert.AreEqual(sampled, headers[HttpTraceContext.Sampled]);
            }
            else
            {
                Assert.False(headers.ContainsKey(HttpTraceContext.Sampled));
            }
        }

        private static void CheckHeaders(NameValueCollection headers, string traceId, string parentSpanId, string spanId, string flags, string sampled, int count)
        {
            Assert.AreEqual(count, headers.Count);

            Assert.AreEqual(traceId, headers[HttpTraceContext.TraceId]);
            Assert.AreEqual(parentSpanId, headers[HttpTraceContext.ParentSpanId]);
            Assert.AreEqual(spanId, headers[HttpTraceContext.SpanId]);
            Assert.AreEqual(flags, headers[HttpTraceContext.Flags]);
            Assert.AreEqual(sampled, headers[HttpTraceContext.Sampled]);
        }

        #endregion

        #region Get/Set inversions

        [TestCase("0000000000000001", "0000000000000000", "00000000000000FA", "0", null, 4)]
        [TestCase("0000000000000001", "0000000000000000", "00000000000000FA", "0", "", 4)]
        [TestCase("0000000000000001", "0000000000000000", null, "0", null, 3)]
        public void GetTraceThenSetHeadersEqualsOriginal(string encodedTraceId, string encodedSpanId, string encodedParentSpanId, string flagsStr, string sampledStr, int expectedHeadersCount)
        {
            Trace parsedTrace;
            Assert.True(HttpTraceContext.TryParseTrace(encodedTraceId, encodedSpanId, encodedParentSpanId, sampledStr, flagsStr, out parsedTrace));

            var recreatedHeaders = new NameValueCollection();
            HttpTraceContext.Set(recreatedHeaders, parsedTrace);

            Assert.AreEqual(expectedHeadersCount, recreatedHeaders.Count);

            Assert.AreEqual(encodedTraceId, recreatedHeaders[HttpTraceContext.TraceId]);
            Assert.AreEqual(encodedParentSpanId, recreatedHeaders[HttpTraceContext.ParentSpanId]);
            Assert.AreEqual(encodedSpanId, recreatedHeaders[HttpTraceContext.SpanId]);
            Assert.AreEqual(flagsStr, recreatedHeaders[HttpTraceContext.Flags]);
        }

        [TestCase(null)]
        [TestCase(9845431L)]
        public void SetHeadersThenGetTraceEqualsOriginal(long? parentSpanId)
        {
            CheckSetHeadersThenGetTrace_Dict(parentSpanId);
            CheckSetHeadersThenGetTrace_NVC(parentSpanId);
        }

        private static void CheckSetHeadersThenGetTrace_Dict(long? parentSpanId)
        {
            var spanId = new SpanId(1, parentSpanId, 250, Flags.Empty());
            var originalTrace = Trace.CreateFromId(spanId);

            var headers = new Dictionary<string, string>();
            HttpTraceContext.Set(headers, originalTrace);

            Trace deserializedTrace;
            Assert.True(HttpTraceContext.TryGet(headers, out deserializedTrace));

            Assert.AreEqual(originalTrace, deserializedTrace);
        }

        private static void CheckSetHeadersThenGetTrace_NVC(long? parentSpanId)
        {
            var spanId = new SpanId(1, parentSpanId, 250, Flags.Empty());
            var originalTrace = Trace.CreateFromId(spanId);

            var headers = new NameValueCollection();
            HttpTraceContext.Set(headers, originalTrace);

            Trace deserializedTrace;
            Assert.True(HttpTraceContext.TryGet(headers, out deserializedTrace));

            Assert.AreEqual(originalTrace, deserializedTrace);
        }

        #endregion

        #region Id Encoding

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

        #endregion

        #region "Flags & Sampled headers relationship"

        [Test]
        public void SampledHeaderFollowFlagsValueForCompatibility()
        {
            var headers = new Dictionary<string, string>();
            var spanNoFlags = Trace.CreateFromId(new SpanId(1, 2, 250, Flags.Empty()));
            HttpTraceContext.Set(headers, spanNoFlags);
            Assert.False(headers.ContainsKey(HttpTraceContext.Sampled)); // no flags then no sampled header

            headers = new Dictionary<string, string>();
            var spanFlagNotSampled = Trace.CreateFromId(new SpanId(1, 2, 250, Flags.Empty().SetNotSampled()));
            HttpTraceContext.Set(headers, spanFlagNotSampled);
            Assert.AreEqual("0", headers[HttpTraceContext.Sampled]); // header sampled to false since flags set to not sampled

            headers = new Dictionary<string, string>();
            var spanFlagSampled = Trace.CreateFromId(new SpanId(1, 2, 250, Flags.Empty().SetSampled()));
            HttpTraceContext.Set(headers, spanFlagSampled);
            Assert.AreEqual("1", headers[HttpTraceContext.Sampled]); // header sampled to true since flags set to sampled
        }

        [TestCase(null, "0", false)]
        [TestCase(null, "1", true)]
        [TestCase(null, "true", true)]
        [TestCase(null, "True", true)]
        [TestCase("0", "1", true)]
        [TestCase("2", "1", true)]
        [TestCase("6", "1", true)]
        [TestCase("0", "0", false)]
        [TestCase("2", "0", false)]
        [TestCase("6", "0", false)]
        [TestCase("6", "false", false)]
        [TestCase("6", "False", false)]
        [TestCase("0", null, false)]
        [TestCase("2", null, false)]
        [TestCase("6", null, true)]
        [Description("If present Sampled header value overrides Flags header")]
        public void SampledHeaderIfPresentOverridesFlags(string flagsStr, string sampledStr, bool isSampledExpected)
        {
            Trace trace;

            var headers = new Dictionary<string, string>
            {
                {HttpTraceContext.TraceId, "0000000000000001"},
                {HttpTraceContext.ParentSpanId, "0000000000000000"},
                {HttpTraceContext.SpanId, "00000000000000FA"},
                {HttpTraceContext.Flags, flagsStr}
            };

            if (sampledStr != null)
            {
                headers[HttpTraceContext.Sampled] = sampledStr;
            }

            Assert.True(HttpTraceContext.TryGet(headers, out trace));

            var flags = trace.CurrentId.Flags;
            Assert.AreEqual(isSampledExpected, flags.IsSampled());
        }

        #endregion

        [Test]
        public void ParsingErrorAreLoggedAndDoesntThrow()
        {
            Trace trace;

            Assert.False(HttpTraceContext.TryParseTrace("44FmalformedTraceId", "00000000000000FA", null, null, null, out trace));

            _mockLogger.Verify(logger => logger.LogWarning(It.Is<string>(s => s.Contains("Couldn't parse trace context. Trace is ignored"))), Times.Once());
        }

    }
}
