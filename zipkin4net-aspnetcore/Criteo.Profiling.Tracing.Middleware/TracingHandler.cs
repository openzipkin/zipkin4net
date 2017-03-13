using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Criteo.Profiling.Tracing.Transport;

namespace Criteo.Profiling.Tracing.Middleware
{
    public class TracingHandler : DelegatingHandler
    {
        private readonly ITraceInjector<HttpHeaders> _injector;

        public TracingHandler(ITraceInjector<HttpHeaders> injector)
        {
            _injector = injector;
        }
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            var trace = Trace.Current;
            if (trace != null)
            {
                _injector.Inject(Trace.Current, request.Headers);
            }
            return base.SendAsync(request, cancellationToken);
        }
    }
}