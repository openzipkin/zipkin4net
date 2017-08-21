using System;
using Microsoft.AspNetCore.Builder;
using zipkin4net;
using zipkin4net.Transport;
using zipkin4net.Utils;
using Microsoft.AspNetCore.Http.Extensions;

namespace zipkin4net.Middleware
{
    public static class TracingMiddleware
    {
        public static void UseTracing(this IApplicationBuilder app, string serviceName)
        {
            var extractor = new ZipkinHttpTraceExtractor();
            app.Use(async (context, next) =>
            {
                Trace trace;
                var request = context.Request;
                if (!extractor.TryExtract(request.Headers, (c, key) => c[key], out trace))
                {
                    trace = Trace.Create();
                }
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
    }
}