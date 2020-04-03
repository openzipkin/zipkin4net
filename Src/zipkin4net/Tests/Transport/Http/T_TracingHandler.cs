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

        [SetUp]
        public void SetUp()
        {
            dispatcher = new Mock<IRecordDispatcher>();

            TraceManager.ClearTracers();
            TraceManager.Stop();
            TraceManager.SamplingRate = 1.0f;
            TraceManager.Start(new VoidLogger(), dispatcher.Object);
        }

        [Test]
        public async Task ShouldLogTagAnnotations()
        {
            // Arrange
            dispatcher
                .Setup(h => h.Dispatch(It.IsAny<Record>()))
                .Returns(true);

            var returnStatusCode = HttpStatusCode.BadRequest;
            var tracingHandler = new TracingHandler("abc")
            {
                InnerHandler = new TestHandler(returnStatusCode)
            };
            httpClient = new HttpClient(tracingHandler);

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

        [Test]
        public async Task ShouldNotLogStatusCodeOnHttpCodeSuccess()
        {
            // Arrange
            var tracingHandler = new TracingHandler("abc")
            {
                InnerHandler = new TestHandler(HttpStatusCode.OK)
            };
            httpClient = new HttpClient(tracingHandler);


            dispatcher
                .Setup(h => h.Dispatch(It.Is<Record>(m =>
                    m.Annotation is TagAnnotation
                    && ((TagAnnotation)m.Annotation).Key == zipkinCoreConstants.HTTP_STATUS_CODE)))
                .Throws(new Exception("HTTP_STATUS_CODE Shouldn't be logged."));

            // Act and assert
            Trace.Current = Trace.Create();
            var uri = new Uri("https://abc.com/");
            var method = HttpMethod.Get;
            await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, uri));
        }

        private class TestHandler : DelegatingHandler
        {
            private HttpStatusCode _returnStatusCode;

            public TestHandler(HttpStatusCode returnStatusCode)
            {
                _returnStatusCode = returnStatusCode;
            }

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return Task.FromResult(new HttpResponseMessage(_returnStatusCode)
                {
                    Content = new StringContent("OK", Encoding.UTF8, "application/json"),
                    RequestMessage = request
                });
            }
        }
    }
}
