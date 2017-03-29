using System;
using Microsoft.AspNetCore.Builder;
using Criteo.Profiling.Tracing;
using Criteo.Profiling.Tracing.Utils;
using Microsoft.AspNetCore.Http.Extensions;

namespace Criteo.Profiling.Tracing.Middleware
{
    public static class TracingMiddleware
    {
        public static void UseTracing(this IApplicationBuilder app, string serviceName)
        {
            var extractor = new Middleware.ZipkinHttpTraceExtractor();
            app.Use(async (context, next) =>
            {
                Trace trace;
                var request = context.Request;
                if (!extractor.TryExtract(request.Headers, out trace))
                {
                    trace = Trace.Create();
                }
                Trace.Current = trace;
                using (new ServerTrace(serviceName, request.Method))
                {
                    trace.Record(Annotations.Tag("http.host", request.Host.ToString()));
                    trace.Record(Annotations.Tag("http.uri", UriHelper.GetDisplayUrl(request)));
                    trace.Record(Annotations.Tag("http.path", request.Path));
                    await TraceHelper.TracedActionAsync(next());
                }
            });
        }
    }
}