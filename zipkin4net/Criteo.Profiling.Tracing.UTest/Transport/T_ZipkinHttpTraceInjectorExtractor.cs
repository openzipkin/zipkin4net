using System.Collections.Generic;
using System.Collections.Specialized;
using Criteo.Profiling.Tracing.Transport;
using NUnit.Framework;

namespace Criteo.Profiling.Tracing.UTest.Transport
{
    [TestFixture]
    internal class T_ZipkinHttpTraceInjectorExtractor
    {

        [TestCase("0000000000000001", "0000000000000000", "00000000000000FA", "0", null, 4)]
        [TestCase("0000000000000001", "0000000000000000", "00000000000000FA", "0", "", 4)]
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

        [TestCase(null)]
        [TestCase(9845431L)]
        public void SetHeadersThenGetTraceEqualsOriginal(long? parentSpanId)
        {
            CheckSetHeadersThenGetTrace_Dict(parentSpanId);
            CheckSetHeadersThenGetTrace_NVC(parentSpanId);
        }

        private static void CheckSetHeadersThenGetTrace_Dict(long? parentSpanId)
        {
            var spanState = new SpanState(1, parentSpanId, 2, SpanFlags.None);
            var originalTrace = Trace.CreateFromId(spanState);

            var headers = new Dictionary<string, string>();
            var zipkinHttpTraceInjector = new ZipkinHttpTraceInjector();
            zipkinHttpTraceInjector.Inject(originalTrace, headers);

            Trace deserializedTrace;
            var extractor = new ZipkinHttpTraceExtractor();
            Assert.True(extractor.TryExtract(headers, out deserializedTrace));

            Assert.AreEqual(originalTrace, deserializedTrace);
            Assert.AreEqual(originalTrace.CurrentSpan.Flags, deserializedTrace.CurrentSpan.Flags);
        }

        private static void CheckSetHeadersThenGetTrace_NVC(long? parentSpanId)
        {
            var spanState = new SpanState(1, parentSpanId, 2, SpanFlags.None);
            var originalTrace = Trace.CreateFromId(spanState);

            var headers = new NameValueCollection();
            var zipkinHttpTraceInjector = new ZipkinHttpTraceInjector();
            zipkinHttpTraceInjector.Inject(originalTrace, headers);

            Trace deserializedTrace;
            var extractor = new ZipkinHttpTraceExtractor();
            Assert.True(extractor.TryExtract(headers, out deserializedTrace));

            Assert.AreEqual(originalTrace, deserializedTrace);
            Assert.AreEqual(originalTrace.CurrentSpan.Flags, deserializedTrace.CurrentSpan.Flags);
        }

    }
}
