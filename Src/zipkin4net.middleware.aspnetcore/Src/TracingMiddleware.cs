using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using zipkin4net.Propagation;

namespace zipkin4net.Middleware
{
    public static class TracingMiddleware
    {
        public static void UseTracing(this IApplicationBuilder app, string serviceName)
        {
            var extractor = Propagations.B3String.Extractor(new HeaderDictionaryGetter());
            app.Use(async (context, next) =>
            {
                var request = context.Request;
                var traceContext = extractor.Extract(request.Headers);
                
                var trace = traceContext == null ? Trace.Create() : Trace.CreateFromId(traceContext);
                Trace.Current = trace;
                using (var serverTrace = new ServerTrace(serviceName, request.Method))
                {
                    trace.Record(Annotations.Tag("http.host", request.Host.ToString()));
                    trace.Record(Annotations.Tag("http.uri", UriHelper.GetDisplayUrl(request)));
                    trace.Record(Annotations.Tag("http.path", request.Path));
                    await serverTrace.TracedActionAsync(next());
                }
            });
        }

        private class HeaderDictionaryGetter : IGetter<IHeaderDictionary, string>
        {
            public string Get(IHeaderDictionary carrier, string key)
            {
                return carrier[key];
            }
        }
    }
}