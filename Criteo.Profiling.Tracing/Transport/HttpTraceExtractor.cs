using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;

namespace Criteo.Profiling.Tracing.Transport
{
    public class HttpTraceExtractor : ITraceExtractor<HttpRequestBase>, ITraceExtractor<NameValueCollection>, ITraceExtractor<IDictionary<string, string>>
    {
        public bool TryExtract(HttpRequestBase transport, out Trace trace)
        {
            return TryExtract(transport.Headers, out trace);
        }

        public bool TryExtract(NameValueCollection transport, out Trace trace)
        {
            return HttpTraceContext.TryGet(transport, out trace);
        }

        public bool TryExtract(IDictionary<string, string> transport, out Trace trace)
        {
            return HttpTraceContext.TryGet(transport, out trace);
        }
    }
}
