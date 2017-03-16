using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Criteo.Profiling.Tracing.Transport;
using Criteo.Profiling.Tracing.Utils;

namespace Criteo.Profiling.Tracing.Middleware
{
    public class TracingHandler : DelegatingHandler
    {
        private readonly ITraceInjector<HttpHeaders> _injector;
        private readonly string _serviceName;

        public TracingHandler(string serviceName, HttpMessageHandler httpMessageHandler = null)
        : this(new Middleware.ZipkinHttpTraceInjector(), serviceName, httpMessageHandler)
        { }

        internal TracingHandler(ITraceInjector<HttpHeaders> injector, string serviceName, HttpMessageHandler httpMessageHandler = null)
        {
            _injector = injector;
            _serviceName = serviceName;
            InnerHandler = httpMessageHandler ?? new HttpClientHandler();
        }
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            using (var clientTrace = new ClientTrace(_serviceName, request.Method.ToString()))
            {
                if (clientTrace.Trace != null)
                {
                    _injector.Inject(clientTrace.Trace, request.Headers);
                }

                return await TraceHelper.TracedActionAsync(base.SendAsync(request, cancellationToken));
            }
        }
    }
}