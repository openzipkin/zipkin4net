using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Criteo.Profiling.Tracing.Middleware;
using common;

namespace frontend
{
    public class Startup : CommonStartup
    {
        protected override void Run(IApplicationBuilder app, IConfiguration config)
        {
            app.Run(async (context) =>
            {
                var callServiceUrl = config["callServiceUrl"];
                using (var httpClient = new HttpClient(new TracingHandler(config["applicationName"])))
                {
                    await Task.Delay(100);
                    var response = await httpClient.GetAsync(callServiceUrl);
                    if (!response.IsSuccessStatusCode)
                    {
                        await context.Response.WriteAsync(response.ReasonPhrase);
                    }
                    else
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        await context.Response.WriteAsync(content);
                    }
                }
            });
        }
    }
}