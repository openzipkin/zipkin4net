using System.Net.Http;
using common;
using System.Threading.Tasks;
using System.Threading;
using Criteo.Profiling.Tracing.Middleware;

namespace frontend
{
    public class Startup : CommonStartup
    {
        protected override HttpMessageHandler GetHandler() => new CallApiHandler();

        class CallApiHandler : HttpMessageHandler
        {
            private static readonly string callServiceUrl = System.Configuration.ConfigurationManager.AppSettings["callServiceUrl"];
            private static readonly string applicationName = System.Configuration.ConfigurationManager.AppSettings["applicationName"];

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                using (var httpClient = new HttpClient(new TracingHandler(applicationName)))
                {
                    var response = await httpClient.GetAsync(callServiceUrl);

                    if (!response.IsSuccessStatusCode)
                    {
                        return response;
                    }

                    var content = await response.Content.ReadAsStringAsync();

                    return request.CreateResponse(System.Net.HttpStatusCode.OK, content);
                }
            }
        }
    }
}