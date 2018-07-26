using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using zipkin4net.Transport.Http;
using common;
using Microsoft.Extensions.DependencyInjection;

namespace frontend
{
    public class Startup : CommonStartup
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient("Tracer").AddHttpMessageHandler(provider =>
                TracingHandler.WithoutInnerHandler(provider.GetService<IConfiguration>()["applicationName"]));
        }

        protected override void Run(IApplicationBuilder app, IConfiguration config)
        {
            app.Run(async (context) =>
            {
                var callServiceUrl = config["callServiceUrl"];
                var clientFactory = app.ApplicationServices.GetService<IHttpClientFactory>();
                using (var httpClient = clientFactory.CreateClient("Tracer"))
                {
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
