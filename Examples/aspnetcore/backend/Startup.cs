using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using zipkin4net.Middleware;
using common;

namespace backend
{
    public class Startup : CommonStartup
    {
        protected override void Run(IApplicationBuilder app, IConfiguration config)
        {
            app.Run(async (context) =>
            {
                await context.Response.WriteAsync(DateTime.Now.ToString());
            });
        }
    }
}
