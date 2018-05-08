using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using System;
using zipkin4net.Propagation;

namespace zipkin4net.Middleware
{
    public static class TracingMiddleware
    {
        public static void UseTracing(this IApplicationBuilder app, string serviceName,
            Func<HttpContext, string> getRpc = null)
        {
            getRpc = getRpc ?? (context => context.Request.Method);
            var extractor = Propagations.B3String.Extractor<IHeaderDictionary>((carrier, key) => carrier[key]);
            app.Use(async (context, next) =>
            {
                var request = context.Request;
                var traceContext = extractor.Extract(request.Headers);
                
                var trace = traceContext == null ? Trace.Create() : Trace.CreateFromId(traceContext);
                Trace.Current = trace;
                using (var serverTrace = new ServerTrace(serviceName, getRpc(context)))
                {
                    if (request.Host.HasValue)
                    {
                        trace.Record(Annotations.Tag("http.host", request.Host.ToString()));
                    }
                    trace.Record(Annotations.Tag("http.uri", UriHelper.GetDisplayUrl(request)));
                    trace.Record(Annotations.Tag("http.path", request.Path));
                    await serverTrace.TracedActionAsync(next());
                }
            });
        }
    }
}