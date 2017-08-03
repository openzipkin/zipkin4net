using System.Net.Http;
using System;
using System.Threading;
using System.Threading.Tasks;
using common;

namespace backend
{
    public class Startup : CommonStartup
    {
        protected override HttpMessageHandler GetHandler() => new DateTimeHandler();
        protected override string GetRouteTemplate() => "api";

        class DateTimeHandler : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
                => Task.FromResult(request.CreateResponse(System.Net.HttpStatusCode.OK, DateTime.Now));
        }
    }
}