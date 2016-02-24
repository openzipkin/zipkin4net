using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;

namespace Criteo.Profiling.Tracing.Transport
{
    public class HttpTraceInjector : ITraceInjector<HttpResponseBase>, ITraceInjector<NameValueCollection>, ITraceInjector<IDictionary<string, string>>
    {
        public bool Inject(Trace trace, HttpResponseBase transport)
        {
            return Inject(trace, transport.Headers);
        }

        public bool Inject(Trace trace, NameValueCollection transport)
        {
            HttpTraceContext.Set(transport, trace);
            return true;
        }

        public bool Inject(Trace trace, IDictionary<string, string> transport)
        {
            HttpTraceContext.Set(transport, trace);
            return true;
        }
    }
}
