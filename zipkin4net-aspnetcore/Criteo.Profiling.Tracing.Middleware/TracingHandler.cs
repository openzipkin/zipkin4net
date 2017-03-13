using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Criteo.Profiling.Tracing.Transport;

namespace Criteo.Profiling.Tracing.Middleware
{
    public class TracingHandler : DelegatingHandler
    {
        private readonly ITraceInjector<HttpHeaders> _injector;
        private readonly string _serviceName;

        public TracingHandler(ITraceInjector<HttpHeaders> injector, string serviceName)
        {
            _injector = injector;
            _serviceName = serviceName;
        }
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            var trace = Trace.Current;
            if (trace != null)
            {
                trace = trace.Child();
                _injector.Inject(trace, request.Headers);
            }
            trace.Record(Annotations.ClientSend());
            trace.Record(Annotations.ServiceName(_serviceName));
            trace.Record(Annotations.Rpc(request.Method.ToString()));
            return base.SendAsync(request, cancellationToken)
                .ContinueWith(t => {
                    trace.Record(Annotations.ClientRecv());
                    return t.Result;
                });
        }
    }
}