using zipkin4net.Annotation;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using zipkin4net.Dispatcher;
using zipkin4net.Middleware.Tests.Helpers;

namespace zipkin4net.Middleware.Tests
{
    using static OwinHelper;
    using static CheckHelper;

    public class WhenOwinMiddlewareIsActive
    {
        private const int DelayInMilliseconds = 5000;
        private const int PollingInterval = 50;

        //See https://github.com/criteo/zipkin4net/commit/14574b36582d184ecba28f746e779c6ff36442b2
        private static readonly bool IsRunningOnMono = Type.GetType("Mono.Runtime") != null;
        private ILogger _logger;
        private ITracer _tracer;
        private Mock<IRecordDispatcher> _dispatcher;

        [SetUp]
        public void Setup()
        {
            _logger = Substitute.For<ILogger>();
            _tracer = Mock.Of<ITracer>();
            _dispatcher = new Mock<IRecordDispatcher>();

            TraceManager.SamplingRate = 1.0f;
            TraceManager.RegisterTracer(_tracer);
            TraceManager.Start(_logger, _dispatcher.Object);
        }

        [TearDown]
        public void TearDown()
        {
            TraceManager.ClearTracers();
            TraceManager.Stop();
        }

        [Test]
        public async Task Check_That_dispatcher_is_called_with_expected_records_on_GET_call()
        {
            //Arrange
            var urlToCall = new Uri("http://testserver/api/values?foo=bar");
            var serviceName = "OwinTest";

            Func<HttpClient, Task<string>> clientCall = async (client) =>
            {
                var response = await client.GetAsync(urlToCall);
                return await response.Content.ReadAsStringAsync();
            };

            //Act
            var responseContent = await Call(DefaultStartup(serviceName), clientCall);
            Assert.IsNotEmpty(responseContent);

            if (!IsRunningOnMono)
            {
                AssertAnnotationReceived<ServerRecv>(_dispatcher);
                AssertAnnotationReceived<ServerSend>(_dispatcher);
                AssertAnnotationReceived<Rpc>(_dispatcher, rpc => rpc.Name == "GET");
                AssertAnnotationReceived<ServiceName>(_dispatcher, serviceNameAnnotation => serviceNameAnnotation.Service == serviceName);
            }
            AssertAnnotationReceived<TagAnnotation>(_dispatcher, tag => has("http.host", urlToCall.Host, tag));
            AssertAnnotationReceived<TagAnnotation>(_dispatcher, tag => has("http.url", urlToCall.AbsoluteUri, tag));
            AssertAnnotationReceived<TagAnnotation>(_dispatcher, tag => has("http.path", urlToCall.AbsolutePath, tag));
        }

        [Test]
        public async Task Check_That_dispatcher_is_called_with_expected_records_on_GET_call_when_using_custom_rpc()
        {
            //Arrange
            var urlToCall = new Uri("http://testserver/api/values?foo=bar");
            var serviceName = "OwinTest";

            Func<HttpClient, Task<string>> clientCall = async (client) =>
            {
                var response = await client.GetAsync(urlToCall);
                return await response.Content.ReadAsStringAsync();
            };

            //Act
            var responseContent = await Call(DefaultStartup(serviceName, context => $"{context.Request.Method}:{context.Request.Path}"), clientCall);
            Assert.IsNotEmpty(responseContent);

            if (!IsRunningOnMono)
            {
                AssertAnnotationReceived<ServerRecv>(_dispatcher);
                AssertAnnotationReceived<ServerSend>(_dispatcher);
                AssertAnnotationReceived<Rpc>(_dispatcher, rpc => rpc.Name == "GET:/api/values");
                AssertAnnotationReceived<ServiceName>(_dispatcher, serviceNameAnnotation => serviceNameAnnotation.Service == serviceName);
            }
            AssertAnnotationReceived<TagAnnotation>(_dispatcher, tag => has("http.host", urlToCall.Host, tag));
            AssertAnnotationReceived<TagAnnotation>(_dispatcher, tag => has("http.url", urlToCall.AbsoluteUri, tag));
            AssertAnnotationReceived<TagAnnotation>(_dispatcher, tag => has("http.path", urlToCall.AbsolutePath, tag));
        }

        [Test]
        public async Task Check_That_dispatcher_is_called_with_expected_records_on_POST_call()
        {
            //Arrange
            var urlToCall = new Uri("http://testserver/api/values");
            var serviceName = "OwinTest";

            Func<HttpClient, Task<string>> clientCall = async (client) =>
            {
                var response = await client.PostAsync(urlToCall, new StringContent(""));
                return await response.Content.ReadAsStringAsync();
            };

            //Act
            var responseContent = await Call(DefaultStartup(serviceName), clientCall);
            Assert.IsNotEmpty(responseContent);

            if (!IsRunningOnMono)
            {
                AssertAnnotationReceived<ServerRecv>(_dispatcher);
                AssertAnnotationReceived<ServerSend>(_dispatcher);
                AssertAnnotationReceived<Rpc>(_dispatcher, rpc => rpc.Name == "POST");
                AssertAnnotationReceived<ServiceName>(_dispatcher, serviceNameAnnotation => serviceNameAnnotation.Service == serviceName);
            }
            AssertAnnotationReceived<TagAnnotation>(_dispatcher, tag => has("http.host", urlToCall.Host, tag));
            AssertAnnotationReceived<TagAnnotation>(_dispatcher, tag => has("http.url", urlToCall.AbsoluteUri, tag));
            AssertAnnotationReceived<TagAnnotation>(_dispatcher, tag => has("http.path", urlToCall.AbsolutePath, tag));
        }

        private static void AssertAnnotationReceived<T>(Mock<IRecordDispatcher> dispatcher)
        {
            AssertAnnotationReceived<T>(dispatcher, annotation => true);
        }

        private static void AssertAnnotationReceived<T>(Mock<IRecordDispatcher> dispatcher, Func<T, bool> assertionOnAnnotation)
        {
            dispatcher.Verify(d => d.Dispatch(It.Is<Record>(r => r.Annotation is T && assertionOnAnnotation((T)r.Annotation))));
        }
    }
}
