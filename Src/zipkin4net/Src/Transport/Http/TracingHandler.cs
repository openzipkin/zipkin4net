using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using zipkin4net.Propagation;
using zipkin4net.Tracers.Zipkin.Thrift;

namespace zipkin4net.Transport.Http
{
    public class TracingHandler : DelegatingHandler
    {
        private readonly IInjector<HttpHeaders> _injector;
        private readonly string _serviceName;
        private readonly Func<HttpRequestMessage, string> _getClientTraceRpc;

        /// <summary>
        /// Create a Tracing Handler
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="httpMessageHandler">if not set or null then it set an <see cref="HttpClientHandler"/> as inner handler</param>
        /// <param name="getClientTraceRpc"></param>
        public TracingHandler(string serviceName, HttpMessageHandler httpMessageHandler = null, Func<HttpRequestMessage, string> getClientTraceRpc = null)
        : this(Propagations.B3String.Injector<HttpHeaders>((carrier, key, value) => carrier.Add(key, value)), serviceName, httpMessageHandler ?? new HttpClientHandler(), getClientTraceRpc)
        { }

        /// <summary>
        /// Create a TracingHandler for injection purpose like HttpClientFactory in AspNetCore
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="getClientTraceRpc"></param>
        /// <returns></returns>
        public static TracingHandler WithoutInnerHandler(string serviceName, Func<HttpRequestMessage, string> getClientTraceRpc = null)
         =>  new TracingHandler(Propagations.B3String.Injector<HttpHeaders>((carrier, key, value) => carrier.Add(key, value)), serviceName, getClientTraceRpc);

        private TracingHandler(IInjector<HttpHeaders> injector, string serviceName, HttpMessageHandler httpMessageHandler, Func<HttpRequestMessage, string> getClientTraceRpc = null)
            : base(httpMessageHandler)
        {
            _injector = injector;
            _serviceName = serviceName;
            _getClientTraceRpc = getClientTraceRpc ?? (request => request.Method.ToString());
        }

        /// <summary>
        /// Constructor used to create the handler without an inner handler.
        /// </summary>
        /// <param name="injector"></param>
        /// <param name="serviceName"></param>
        /// <param name="getClientTraceRpc"></param>
        private TracingHandler(IInjector<HttpHeaders> injector, string serviceName, Func<HttpRequestMessage, string> getClientTraceRpc = null)
        {
            _injector = injector;
            _serviceName = serviceName;
            _getClientTraceRpc = getClientTraceRpc ?? (request => request.Method.ToString());
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            using (var clientTrace = new ClientTrace(_serviceName, _getClientTraceRpc(request)))
            {
                if (clientTrace.Trace != null)
                {
                    _injector.Inject(clientTrace.Trace.CurrentSpan, request.Headers);
                }

                var result = await clientTrace.TracedActionAsync(base.SendAsync(request, cancellationToken));

                if (clientTrace.Trace != null)
                {
                    clientTrace.AddAnnotation(Annotations.Tag(zipkinCoreConstants.HTTP_HOST, result.RequestMessage.RequestUri.Host));
                    clientTrace.AddAnnotation(Annotations.Tag(zipkinCoreConstants.HTTP_PATH, result.RequestMessage.RequestUri.LocalPath));
                    clientTrace.AddAnnotation(Annotations.Tag(zipkinCoreConstants.HTTP_METHOD, result.RequestMessage.Method.Method));
                    if (!result.IsSuccessStatusCode)
                    {
                        clientTrace.AddAnnotation(Annotations.Tag(zipkinCoreConstants.HTTP_STATUS_CODE, ((int)result.StatusCode).ToString()));
                    }
                }

                return result;
            }
        }
    }
}
