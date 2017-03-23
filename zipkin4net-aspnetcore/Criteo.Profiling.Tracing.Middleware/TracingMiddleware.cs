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
                if (!extractor.TryExtract(context.Request.Headers, out trace))
                {
                    trace = Trace.Create();
                }

                Trace.Current = trace;

                using (new ServerTrace(serviceName, context.Request.Method))
                {
                    await TraceHelper.TracedActionAsync(next());
                }
            });
        }
    }
}