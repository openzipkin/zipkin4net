using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using zipkin4net.Propagation;

namespace zipkin4net.Transport
{
    /**
     * Extract B3 headers from HTTP headers.
     */
    [Obsolete("Please use Propagation.IPropagation instead")]
    public class ZipkinHttpTraceExtractor : ITraceExtractor<NameValueCollection>, ITraceExtractor<IDictionary<string, string>>, ITraceExtractor
    {
        private const int traceId64BitsSerializationLength = 16;

        private class ZipkinHttpTraceExtractorGetter<C> : IGetter<C, string>
        {
            private readonly Func<C, string, string> _extractor;

            internal ZipkinHttpTraceExtractorGetter(Func<C, string, string> extractor)
            {
                _extractor = extractor;
            }

            public string Get(C carrier, string key)
            {
                return _extractor(carrier, key);
            }
        }

        private static IGetter<NameValueCollection, string> NameValueCollectionGetter = new ZipkinHttpTraceExtractorGetter<NameValueCollection>((c, key) => c[key]);
        private static IGetter<IDictionary<string, string>, string> DictionaryGetter = new ZipkinHttpTraceExtractorGetter<IDictionary<string, string>>((c, key) =>
        {
            string value;
            return c.TryGetValue(key, out value) ? value : null;
        });

        private static readonly IExtractor<NameValueCollection> NameValueCollectionExtractor = Propagations.B3String.Extractor(NameValueCollectionGetter);
        private static readonly IExtractor<IDictionary<string, string>> DictionaryExtractor = Propagations.B3String.Extractor(DictionaryGetter);

        public bool TryExtract<TE>(TE carrier, Func<TE, string, string> extractor, out Trace trace)
        {
            return TryParseTrace(
                extractor(carrier, ZipkinHttpHeaders.TraceId),
                extractor(carrier, ZipkinHttpHeaders.SpanId),
                extractor(carrier, ZipkinHttpHeaders.ParentSpanId),
                extractor(carrier, ZipkinHttpHeaders.Sampled),
                extractor(carrier, ZipkinHttpHeaders.Flags),
                out trace);
        }

        public static bool TryParseTrace(string encodedTraceId, string encodedSpanId, string encodedParentSpanId, string sampledStr, string flagsStr, out Trace trace)
        {
            var traceContext = B3Extractor<NameValueCollection, string>.TryParseTrace(encodedTraceId, encodedSpanId, encodedParentSpanId, sampledStr, flagsStr);
            return TryCreateTraceFromTraceContext(traceContext, out trace);
        }

        public bool TryExtract(NameValueCollection carrier, out Trace trace)
        {
            return TryExtract(carrier, NameValueCollectionExtractor, out trace);
        }

        public bool TryExtract(IDictionary<string, string> carrier, out Trace trace)
        {
            return TryExtract(carrier, DictionaryExtractor, out trace);
        }

        private static bool TryExtract<C>(C carrier, IExtractor<C> extractor, out Trace trace)
        {
            ITraceContext traceContext = default(SpanState);
            traceContext = extractor.Extract(carrier);
            return TryCreateTraceFromTraceContext(traceContext, out trace);
        }

        private static bool TryCreateTraceFromTraceContext(ITraceContext traceContext, out Trace trace)
        {
            if (traceContext == default(SpanState))
            {
                trace = default(Trace);
                return false;
            }
            trace = Trace.CreateFromId(traceContext);
            return true;
        }
    }
}
