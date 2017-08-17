using Criteo.Profiling.Tracing.Transport;
using Microsoft.Owin.Testing;
using Owin;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Criteo.Profiling.Tracing.Middleware.Tests.Helpers
{
    static class OwinHelper
    {
        internal static async Task Call(Action<IAppBuilder> startup, Func<HttpClient, Task> clientCall)
        {
            using (var server = TestServer.Create(startup))
            {
                using (var client = new HttpClient(server.Handler))
                {
                    await clientCall(client);
                }
            }
        }
        internal static Action<IAppBuilder> DefaultStartup(string serviceName, ITraceExtractor traceExtractor)
        {
            return
                app =>
                {
                    app.UseZipkinTracer(serviceName, traceExtractor);

                    app.Run(async context =>
                    {
                        context.Response.ContentType = "text/plain";
                        await context.Response.WriteAsync(DateTime.Now.ToString());
                    });
                };
        }
    }
}
