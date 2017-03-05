using Criteo.Profiling.Tracing.Tracers.Zipkin;
using System.Net.Http;

namespace Criteo.Profiling.Tracing.Transport.Http
{
    public class HttpZipkinSender : IZipkinSender
    {
        private string _zipkinCollectorUrl;
        private HttpClient _httpClient;

        public HttpZipkinSender(string zipkinCollectorUrl)
        : this(new HttpClient(), zipkinCollectorUrl)
        {}

        internal HttpZipkinSender(HttpClient httplient, string zipkinCollectorUrl)
        {
            _zipkinCollectorUrl = ForgeSpansEndpoint(zipkinCollectorUrl);
            _httpClient = httplient;
        }

        public void Send(byte[] data)
        {
            var content = new ByteArrayContent(data);
            content.Headers.Add("Content-Type", "application/x-thrift");
            content.Headers.Add("Content-Length", data.Length.ToString());
            _httpClient.PostAsync(_zipkinCollectorUrl, content);
        }

        private static string ForgeSpansEndpoint(string url)
        {
            string spansEndPoint = "api/v1/spans";
            if (!url.EndsWith("/"))
            {
                spansEndPoint = "/" + spansEndPoint;
            }
            return url + spansEndPoint;
        }
    }
}