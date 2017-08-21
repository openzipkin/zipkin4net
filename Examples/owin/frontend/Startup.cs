using System.Net.Http;
using common;
using System.Threading.Tasks;
using System.Threading;
using zipkin4net.Transport.Http;
using Microsoft.Owin;
using System;

namespace frontend
{
    public class Startup : CommonStartup
    {
        private static readonly string callServiceUrl = System.Configuration.ConfigurationManager.AppSettings["callServiceUrl"];
        private static readonly string applicationName = System.Configuration.ConfigurationManager.AppSettings["applicationName"];

        protected override async Task RunHandler(IOwinContext context)
        {
            if (!context.Request.Path.HasValue ||
                !context.Request.Path.Value.Equals("/", StringComparison.InvariantCultureIgnoreCase))
            {
                context.Response.StatusCode = 404;
                return;
            }

            using (var httpClient = new HttpClient(new TracingHandler(applicationName)))
            {
                var response = await httpClient.GetAsync(callServiceUrl);
                var content = await response.Content.ReadAsStringAsync();

                await context.Response.WriteAsync(content);
            }
        }
    }
}