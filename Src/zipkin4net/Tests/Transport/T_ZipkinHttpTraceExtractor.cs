using System.Collections.Generic;
using System.Collections.Specialized;
using zipkin4net.Transport;
using Moq;
using NUnit.Framework;

namespace zipkin4net.UTest.Transport
{
    [TestFixture]
    internal class T_ZipkinHttpTraceExtractor
    {

        private readonly ZipkinHttpTraceExtractor _extractor = new ZipkinHttpTraceExtractor();

        private Mock<ILogger> _mockLogger;

        [SetUp]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger>();
            TraceManager.Start(_mockLogger.Object);
        }

        [Test]
        [Description("Required headers are traceId and spanId")]
        public void TryGetFailsWithoutRequiredHeaders()
        {
            Trace receivedTrace;

            // NameValueCollection
            Assert.False(_extractor.TryExtract(new NameValueCollection(), out receivedTrace));
            Assert.IsNull(receivedTrace);

            // IDictionnary
            Assert.False(_extractor.TryExtract(new Dictionary<string, string>(), out receivedTrace));
            Assert.IsNull(receivedTrace);

            Assert.False(_extractor.TryExtract(new Dictionary<string, string> { { ZipkinHttpHeaders.TraceId, "0000000000000001" } }, out receivedTrace));
            Assert.IsNull(receivedTrace);

            Assert.False(_extractor.TryExtract(new Dictionary<string, string> { { ZipkinHttpHeaders.SpanId, "0000000000000001" } }, out receivedTrace));
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
            Assert.False(ZipkinHttpTraceExtractor.TryParseTrace(encodedTraceId, encodedSpanId, encodedParentSpanId: "0000000000000000", sampledStr: "1", flagsStr: "0", trace: out trace));
        }


        [TestCase("0000000000000000")]
        [TestCase(null)]
        public void ShouldParseTraceWithoutFlags(string encodedParentSpanId)
        {
            Trace trace;
            Assert.True(ZipkinHttpTraceExtractor.TryParseTrace(encodedTraceId: "0000000000000001", encodedSpanId: "00000000000000FA", encodedParentSpanId: encodedParentSpanId, sampledStr: null, flagsStr: null, trace: out trace));

            Assert.AreEqual(1L, trace.CurrentSpan.TraceId);
            Assert.AreEqual(SpanState.NoTraceIdHigh, trace.CurrentSpan.TraceIdHigh);
            Assert.AreEqual(250, trace.CurrentSpan.SpanId);
            Assert.IsNull(trace.CurrentSpan.Sampled);
            Assert.IsFalse(trace.CurrentSpan.Debug);

            var expectedParentSpanId = (encodedParentSpanId == null) ? (long?)null : 0L;
            Assert.AreEqual(expectedParentSpanId, trace.CurrentSpan.ParentSpanId);
        }

        [Test]
        public void ParsingErrorAreLoggedAndDoesntThrow()
        {
            Trace trace;

            Assert.False(ZipkinHttpTraceExtractor.TryParseTrace("44FmalformedTraceId", "00000000000000FA", null, null, null, out trace));

            _mockLogger.Verify(logger => logger.LogWarning(It.Is<string>(s => s.Contains("Couldn't parse trace context. Trace is ignored"))), Times.Once());
        }

        [TestCase(null, "0", false)]
        [TestCase(null, "1", true)]
        [TestCase("0", "1", true)]
        [TestCase("2", "1", true)]
        [TestCase("6", "1", true)]
        [TestCase("0", "0", false)]
        [TestCase("2", "0", false)]
        [TestCase("6", "0", false)]
        [TestCase("0", null, null)]
        [TestCase("2", null, false)]
        [TestCase("6", null, true)]
        [Description("If present Sampled header value overrides Flags header")]
        public void SampledHeaderIfPresentOverridesFlags(string flagsStr, string sampledStr, bool? expectedSampled)
        {
            var headers = new Dictionary<string, string>
            {
                {ZipkinHttpHeaders.TraceId, "0000000000000001"},
                {ZipkinHttpHeaders.ParentSpanId, "0000000000000000"},
                {ZipkinHttpHeaders.SpanId, "00000000000000FA"},
                {ZipkinHttpHeaders.Flags, flagsStr}
            };

            if (sampledStr != null)
            {
                headers[ZipkinHttpHeaders.Sampled] = sampledStr;
            }

            Trace trace;
            Assert.True(_extractor.TryExtract(headers, out trace));

            Assert.AreEqual(expectedSampled, trace.CurrentSpan.Sampled);
        }
    }
}
