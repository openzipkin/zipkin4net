using Moq;
using NUnit.Framework;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using zipkin4net.Annotation;
using zipkin4net.Dispatcher;
using zipkin4net.Logger;
using zipkin4net.Tracers.Zipkin.Thrift;
using zipkin4net.Transport.Http;

namespace zipkin4net.UTest.Transport.Http
{
    [TestFixture]
    public class T_TracingHandler
    {
        private HttpClient httpClient;
        private Mock<IRecordDispatcher> dispatcher;
        private static HttpStatusCode returnStatusCode = HttpStatusCode.OK;

        [SetUp]
        public void SetUp()
        {
            dispatcher = new Mock<IRecordDispatcher>();

            TraceManager.ClearTracers();
            TraceManager.Stop();
            TraceManager.SamplingRate = 1.0f;
            TraceManager.Start(new VoidLogger(), dispatcher.Object);

            var tracingHandler = new TracingHandler("abc")
            {
                InnerHandler = new TestHandler()
            };
            httpClient = new HttpClient(tracingHandler);
        }

        [Test]
        public async Task ShouldLogTagAnnotations()
        {
            // Arrange
            dispatcher
                .Setup(h => h.Dispatch(It.IsAny<Record>()))
                .Returns(true);

            // Act
            Trace.Current = Trace.Create();
            var uri = new Uri("https://abc.com/");
            var method = HttpMethod.Get;
            await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, uri));

            // Assert
            dispatcher
                .Verify(h =>
                    h.Dispatch(It.Is<Record>(m =>
                        m.Annotation is TagAnnotation
                        && ((TagAnnotation)m.Annotation).Key == zipkinCoreConstants.HTTP_HOST
                        && ((TagAnnotation)m.Annotation).Value.ToString() == uri.Host)));

            dispatcher
                .Verify(h =>
                    h.Dispatch(It.Is<Record>(m =>
                        m.Annotation is TagAnnotation
                        && ((TagAnnotation)m.Annotation).Key == zipkinCoreConstants.HTTP_PATH
                        && ((TagAnnotation)m.Annotation).Value.ToString() == uri.LocalPath)));

            dispatcher
                .Verify(h =>
                    h.Dispatch(It.Is<Record>(m =>
                        m.Annotation is TagAnnotation
                        && ((TagAnnotation)m.Annotation).Key == zipkinCoreConstants.HTTP_METHOD
                        && ((TagAnnotation)m.Annotation).Value.ToString() == method.Method)));

            dispatcher
                .Verify(h =>
                    h.Dispatch(It.Is<Record>(m =>
                        m.Annotation is TagAnnotation
                        && ((TagAnnotation)m.Annotation).Key == zipkinCoreConstants.HTTP_STATUS_CODE
                        && ((TagAnnotation)m.Annotation).Value.ToString() == ((int)returnStatusCode).ToString())));
        }

        private class TestHandler : DelegatingHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return Task.FromResult(new HttpResponseMessage(returnStatusCode)
                {
                    Content = new StringContent("OK", Encoding.UTF8, "application/json"),
                    RequestMessage = request
                });
            }
        }
    }
}