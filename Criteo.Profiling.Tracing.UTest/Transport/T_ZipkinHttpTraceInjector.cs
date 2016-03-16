using System.Collections.Generic;
using System.Collections.Specialized;
using Criteo.Profiling.Tracing.Transport;
using NUnit.Framework;

namespace Criteo.Profiling.Tracing.UTest.Transport
{
    [TestFixture]
    class T_ZipkinHttpTraceInjector
    {
        private readonly ZipkinHttpTraceInjector _injector = new ZipkinHttpTraceInjector();

        [TestCase(SpanFlags.None, null)]
        [TestCase(SpanFlags.SamplingKnown, "0")]
        [TestCase(SpanFlags.SamplingKnown | SpanFlags.Sampled, "1")]
        public void SampledHeaderFollowFlagsValueForCompatibility(SpanFlags flags, string expectedHeader)
        {
            var spanFlagNotSampled = Trace.CreateFromId(new SpanState(1, 2, 250, flags));

            var headers = new Dictionary<string, string>();
            _injector.Inject(spanFlagNotSampled, headers);

            if (expectedHeader != null)
            {
                Assert.AreEqual(expectedHeader, headers[ZipkinHttpHeaders.Sampled]);
            }
            else
            {
                Assert.IsFalse(headers.ContainsKey(ZipkinHttpHeaders.Sampled));
            }
        }

        [TestCase("0000000000000001", 0L, "0000000000000000", "00000000000000FA", true, "6", "1", 5)]
        [TestCase("0000000000000001", 0L, "0000000000000000", "00000000000000FA", false, "0", null, 4)]
        [TestCase("0000000000000001", null, null, "00000000000000FA", true, "6", "1", 4)]
        public void HeadersAreCorrectlySet(string expectedTraceId, long? parentSpanId, string expectedParentSpanId, string expectedSpanId, bool setSampled, string expectedFlags, string expectedSampled, int expectedCount)
        {
            var spanState = new SpanState(1, parentSpanId, 250, setSampled ? (SpanFlags.SamplingKnown | SpanFlags.Sampled) : SpanFlags.None);
            var trace = Trace.CreateFromId(spanState);

            var headersNvc = new NameValueCollection();
            _injector.Inject(trace, headersNvc);
            CheckHeaders(headersNvc, expectedTraceId, expectedParentSpanId, expectedSpanId, expectedFlags, expectedSampled, expectedCount);

            var headersDict = new Dictionary<string, string>();
            _injector.Inject(trace, headersDict);
            CheckHeaders(headersDict, expectedTraceId, expectedParentSpanId, expectedSpanId, expectedFlags, expectedSampled, expectedCount);
        }

        private static void CheckHeaders(IReadOnlyDictionary<string, string> headers, string traceId, string parentSpanId, string spanId, string flags, string sampled, int count)
        {
            Assert.AreEqual(count, headers.Count);

            // Required fields
            Assert.AreEqual(traceId, headers[ZipkinHttpHeaders.TraceId]);
            Assert.AreEqual(spanId, headers[ZipkinHttpHeaders.SpanId]);
            Assert.AreEqual(flags, headers[ZipkinHttpHeaders.Flags]);

            // Optional fields
            if (parentSpanId != null)
            {
                Assert.AreEqual(parentSpanId, headers[ZipkinHttpHeaders.ParentSpanId]);
            }
            else
            {
                Assert.False(headers.ContainsKey(ZipkinHttpHeaders.ParentSpanId));
            }

            if (sampled != null)
            {
                Assert.AreEqual(sampled, headers[ZipkinHttpHeaders.Sampled]);
            }
            else
            {
                Assert.False(headers.ContainsKey(ZipkinHttpHeaders.Sampled));
            }
        }

        private static void CheckHeaders(NameValueCollection headers, string traceId, string parentSpanId, string spanId, string flags, string sampled, int count)
        {
            Assert.AreEqual(count, headers.Count);

            Assert.AreEqual(traceId, headers[ZipkinHttpHeaders.TraceId]);
            Assert.AreEqual(parentSpanId, headers[ZipkinHttpHeaders.ParentSpanId]);
            Assert.AreEqual(spanId, headers[ZipkinHttpHeaders.SpanId]);
            Assert.AreEqual(flags, headers[ZipkinHttpHeaders.Flags]);
            Assert.AreEqual(sampled, headers[ZipkinHttpHeaders.Sampled]);
        }
    }
}
