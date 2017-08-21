using Moq;
using NUnit.Framework;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using zipkin4net.Transport.Http;

namespace zipkin4net.UTest.Transport.Http
{
    [TestFixture]
    public class T_HttpZipkinSender
    {
        private const string url = "http://localhost";
        private const string contentType = "application/json";
        private Mock<FakeHttpMessageHandler> mockMessageHandler;
        private HttpClient httpClient;
        private static byte[] content = Encoding.ASCII.GetBytes("data");

        [SetUp]
        public void SetUp()
        {
            mockMessageHandler = new Mock<FakeHttpMessageHandler>() { CallBase = true };
            httpClient = new HttpClient(mockMessageHandler.Object);
        }

        [Test]
        public void sendDataShouldSendOnSpansEndPoint()
        {
            var contentType = "application/x-thrift";
            var sender = new HttpZipkinSender(httpClient, url, contentType);
            sender.Send(content);
            mockMessageHandler.Verify(h => h.Send(It.Is<HttpRequestMessage>(
                m => m.RequestUri.Equals(url + "/api/v1/spans")
                && m.Content.Headers.GetValues("Content-Type").Contains(contentType)
                && m.Content.Headers.GetValues("Content-Length").Contains(content.Length.ToString())
                && m.Method == HttpMethod.Post
            )));
        }

        [Test]
        public void invalidUrlShouldThrowWhenSending()
        {
            var sender = new HttpZipkinSender(httpClient, "url", contentType);
            Assert.Throws<InvalidOperationException>(() => sender.Send(content));
        }

        [Test]
        public void sendDataShouldNotAddASlashIfAlreadyPresent()
        {
            var url = "http://localhost/";
            var sender = new HttpZipkinSender(httpClient, url, contentType);
            sender.Send(content);
            mockMessageHandler.Verify(h => h.Send(It.Is<HttpRequestMessage>(
                m => m.RequestUri.Equals(url + "api/v1/spans")
            )));
        }

        public abstract class FakeHttpMessageHandler : HttpMessageHandler
        {
            public virtual HttpResponseMessage Send(HttpRequestMessage request)
            {
                return new Mock<HttpResponseMessage>().Object;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
            {
                return Task.FromResult(Send(request));
            }
        }
    }
}