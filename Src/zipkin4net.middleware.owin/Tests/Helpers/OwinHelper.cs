using Microsoft.Owin.Testing;
using Owin;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace zipkin4net.Middleware.Tests.Helpers
{
    static class OwinHelper
    {
        internal static async Task<string> Call(Action<IAppBuilder> startup, Func<HttpClient, Task<string>> clientCall)
        {
            using (var server = TestServer.Create(startup))
            {
                using (var client = new HttpClient(server.Handler))
                {
                    return await clientCall(client);
                }
            }
        }
        internal static Action<IAppBuilder> DefaultStartup(string serviceName)
        {
            return
                app =>
                {
                    app.UseZipkinTracer(serviceName);

                    app.Run(async context =>
                    {
                        context.Response.ContentType = "text/plain";
                        await context.Response.WriteAsync(DateTime.Now.ToString());
                    });
                };
        }
    }
}
