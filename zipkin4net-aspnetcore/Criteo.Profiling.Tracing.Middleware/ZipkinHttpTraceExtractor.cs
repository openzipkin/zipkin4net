using Microsoft.AspNetCore.Http;
using Criteo.Profiling.Tracing;
using Criteo.Profiling.Tracing.Transport;

namespace Criteo.Profiling.Tracing.Middleware
{
    public class ZipkinHttpTraceExtractor : ITraceExtractor<IHeaderDictionary>
    {
        public bool TryExtract(IHeaderDictionary carrier, out Trace trace)
        {
            return Criteo.Profiling.Tracing.Transport.ZipkinHttpTraceExtractor.TryParseTrace(carrier[ZipkinHttpHeaders.TraceId],
               carrier[ZipkinHttpHeaders.SpanId],
               carrier[ZipkinHttpHeaders.ParentSpanId],
               carrier[ZipkinHttpHeaders.Sampled],
               carrier[ZipkinHttpHeaders.Flags],
               out trace);
        }
    }
}