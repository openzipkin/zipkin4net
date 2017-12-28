using zipkin4net.Tracers.Zipkin;
using System;
using System.Net.Http;

namespace zipkin4net.Transport.Http
{
    public class HttpZipkinSender : IZipkinSender
    {
        private string _zipkinCollectorUrl;
        private HttpClient _httpClient;
        private readonly string _contentType;

        public HttpZipkinSender(string zipkinCollectorUrl, string contentType)
        : this(new HttpClient(), zipkinCollectorUrl, contentType)
        {}

        public HttpZipkinSender(HttpClient httpClient, string zipkinCollectorUrl, string contentType)
        {
            _zipkinCollectorUrl = ForgeSpansEndpoint(zipkinCollectorUrl);
            _httpClient = httpClient;
            _contentType = contentType;
        }

        public void Send(byte[] data)
        {
            var content = new ByteArrayContent(data);
            content.Headers.Add("Content-Type", _contentType);
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
