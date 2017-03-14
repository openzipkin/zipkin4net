using System;
using Microsoft.AspNetCore.Builder;
using Criteo.Profiling.Tracing;

namespace Criteo.Profiling.Tracing.Middleware
{
    public static class TracingMiddleware
    {
        public static void UseTracing(this IApplicationBuilder app, string serviceName)
        {
            var extractor = new Middleware.ZipkinHttpTraceExtractor();
            app.Use(async (context, next) => {
                Trace trace;
                if (!extractor.TryExtract(context.Request.Headers, out trace))
                {
                    trace = Trace.Create();
                }
                Trace.Current = trace;
                trace.Record(Annotations.ServerRecv());
                trace.Record(Annotations.ServiceName(serviceName));
                trace.Record(Annotations.Rpc(context.Request.Method));
                try
                {
                    await next.Invoke();
                }
                catch (System.Exception e)
                {
                    trace.Record(Annotations.Tag("error", e.Message));
                    throw;
                }
                finally
                {
                    trace.Record(Annotations.ServerSend());
                }
            });
        }
    }
}