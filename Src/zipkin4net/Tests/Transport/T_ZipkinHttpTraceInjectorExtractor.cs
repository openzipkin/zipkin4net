using System.Collections.Generic;
using System.Collections.Specialized;
using zipkin4net.Transport;
using zipkin4net.Utils;
using NUnit.Framework;

namespace zipkin4net.UTest.Transport
{
    [TestFixture]
    internal class T_ZipkinHttpTraceInjectorExtractor
    {

        [TestCase("0000000000000001", "0000000000000000", "00000000000000fa", "0", null, 4)]
        [TestCase("0000000000000001", "0000000000000000", "00000000000000fa", "0", "", 4)]
        [TestCase("0000000000000001", "0000000000000000", null, "0", null, 3)]
        public void GetTraceThenSetHeadersEqualsOriginal(string encodedTraceId, string encodedSpanId, string encodedParentSpanId, string flagsStr, string sampledStr, int expectedHeadersCount)
        {
            Trace parsedTrace;
            Assert.True(ZipkinHttpTraceExtractor.TryParseTrace(encodedTraceId, encodedSpanId, encodedParentSpanId, sampledStr, flagsStr, out parsedTrace));

            var recreatedHeaders = new NameValueCollection();
            var injector = new ZipkinHttpTraceInjector();
            injector.Inject(parsedTrace, recreatedHeaders);

            Assert.AreEqual(expectedHeadersCount, recreatedHeaders.Count);

            Assert.AreEqual(encodedTraceId, recreatedHeaders[ZipkinHttpHeaders.TraceId]);
            Assert.AreEqual(encodedParentSpanId, recreatedHeaders[ZipkinHttpHeaders.ParentSpanId]);
            Assert.AreEqual(encodedSpanId, recreatedHeaders[ZipkinHttpHeaders.SpanId]);
            Assert.AreEqual(flagsStr, recreatedHeaders[ZipkinHttpHeaders.Flags]);
        }

        [Test]
        public void Supports128BitsTraceId()
        {
            var traceIdHigh = 1L;
            var traceId = 2L;
            var encodedTraceId = NumberUtils.EncodeLongToLowerHexString(traceIdHigh) + NumberUtils.EncodeLongToLowerHexString(traceId);

            Trace parsedTrace;
            Assert.True(ZipkinHttpTraceExtractor.TryParseTrace(encodedTraceId, "0000000000000000", "0000000000000000", null, "", out parsedTrace));
            Assert.AreEqual(traceIdHigh, parsedTrace.CurrentSpan.TraceIdHigh);
            Assert.AreEqual(traceId, parsedTrace.CurrentSpan.TraceId);
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
            var spanState = new SpanState(2, 1, parentSpanId, 2, isSampled: null, isDebug: false);
            var originalTrace = Trace.CreateFromId(spanState);

            var headers = new Dictionary<string, string>();
            var zipkinHttpTraceInjector = new ZipkinHttpTraceInjector();
            zipkinHttpTraceInjector.Inject(originalTrace, headers);

            Trace deserializedTrace;
            var extractor = new ZipkinHttpTraceExtractor();
            Assert.True(extractor.TryExtract(headers, out deserializedTrace));

            Assert.AreEqual(originalTrace, deserializedTrace);
        }

        private static void CheckSetHeadersThenGetTrace_NVC(long? parentSpanId)
        {
            var spanState = new SpanState(2, 1, parentSpanId, 2, isSampled: null, isDebug: false);
            var originalTrace = Trace.CreateFromId(spanState);

            var headers = new NameValueCollection();
            var zipkinHttpTraceInjector = new ZipkinHttpTraceInjector();
            zipkinHttpTraceInjector.Inject(originalTrace, headers);

            Trace deserializedTrace;
            var extractor = new ZipkinHttpTraceExtractor();
            Assert.True(extractor.TryExtract(headers, out deserializedTrace));

            Assert.AreEqual(originalTrace, deserializedTrace);
        }
    }
}
