using System;
using Microsoft.AspNetCore.Builder;
using Criteo.Profiling.Tracing;
using Criteo.Profiling.Tracing.Utils;

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
                    trace.Record(Annotations.Tag("http.uri", request.Path));
                    await TraceHelper.TracedActionAsync(next());
                }
            });
        }
    }
}