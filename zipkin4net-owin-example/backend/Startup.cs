using System;
using System.Threading.Tasks;
using common;
using Microsoft.Owin;

namespace backend
{
    public class Startup : CommonStartup
    {
        protected override async Task RunHandler(IOwinContext context)
        {
            if (!context.Request.Path.HasValue ||
                !context.Request.Path.Value.StartsWith("/api", StringComparison.InvariantCultureIgnoreCase))
            {
                context.Response.StatusCode = 404;
                return;
            }

            context.Response.ContentType = "text/plain";
            await context.Response.WriteAsync(DateTime.Now.ToString());
        }
    }
}