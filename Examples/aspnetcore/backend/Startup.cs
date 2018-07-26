using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using common;
using Microsoft.Extensions.DependencyInjection;

namespace backend
{
    public class Startup : CommonStartup
    {
        public override void ConfigureServices(IServiceCollection services)
        {
        }

        protected override void Run(IApplicationBuilder app, IConfiguration config)
        {
            app.Run(async (context) =>
            {
                await context.Response.WriteAsync(DateTime.Now.ToString());
            });
        }
    }
}
