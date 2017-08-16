using System.Net.Http;
using System.Threading.Tasks;

namespace Criteo.Profiling.Tracing.Transport.Http
{
    public class TracingHandler : DelegatingHandler
    {
        private readonly ITraceInjector _injector;
        private readonly string _serviceName;

        public TracingHandler(string serviceName, HttpMessageHandler httpMessageHandler = null)
        : this(new ZipkinHttpTraceInjector(), serviceName, httpMessageHandler)
        { }

        internal TracingHandler(ITraceInjector injector, string serviceName, HttpMessageHandler httpMessageHandler = null)
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
                    _injector.Inject(clientTrace.Trace, request.Headers, (c, key, value) => c.Add(key, value));
                }
                return await clientTrace.TracedActionAsync(base.SendAsync(request, cancellationToken));
            }
        }
    }
}
