using zipkin4net.Annotation;
using zipkin4net.Tracers;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using zipkin4net.Middleware.Tests.Helpers;
using zipkin4net.Transport;
using Microsoft.Owin;

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
        private InMemoryTracer _tracer;
        private ITraceExtractor _traceExtractor;

        [SetUp]
        public void Setup()
        {
            _logger = Substitute.For<ILogger>();
            _tracer = new InMemoryTracer();
            _traceExtractor = Substitute.For<ITraceExtractor>();


            TraceManager.SamplingRate = 1.0f;
            TraceManager.RegisterTracer(_tracer);
            TraceManager.Start(_logger);
        }

        [TearDown]
        public void TearDown()
        {
            TraceManager.ClearTracers();
        }

        [Test]
        public async Task Check_That_dispatcher_is_called_with_expected_records_on_GET_call()
        {
            //Arrange
            var urlToCall = new Uri("http://testserver/api/values?foo=bar");
            var serviceName = "OwinTest";

            Func<HttpClient, Task> clientCall = async (client) =>
            {
                var response = await client.GetAsync(urlToCall);
                var result = await response.Content.ReadAsStringAsync();
            };

            //Act
            await Call(DefaultStartup(serviceName, _traceExtractor), clientCall);

            //Assert
            Trace trace = null;
            _traceExtractor.ReceivedWithAnyArgs(1).TryExtract(Arg.Any<IHeaderDictionary>(), Arg.Any<Func<IHeaderDictionary, string, string>>(), out trace);
            var records = _tracer.Records;

            if (!IsRunningOnMono)
            {
                Assert.That(records.Any(r => r.Annotation is ServerRecv), Is.True.After(DelayInMilliseconds, PollingInterval));
                Assert.That(records.Any(r => r.Annotation is ServerSend), Is.True.After(DelayInMilliseconds, PollingInterval));
                Assert.That(records.Any(r => r.Annotation is Rpc && ((Rpc)r.Annotation).Name == "GET"), Is.True.After(DelayInMilliseconds, PollingInterval));
                Assert.That(records.Any(r => r.Annotation is ServiceName && ((ServiceName)r.Annotation).Service == serviceName), Is.True.After(DelayInMilliseconds, PollingInterval));
            }
            Assert.That(records.Any(r => r.Annotation is TagAnnotation && has("http.host", urlToCall.Host, (TagAnnotation)r.Annotation)), Is.True.After(DelayInMilliseconds, PollingInterval));
            Assert.That(records.Any(r => r.Annotation is TagAnnotation && has("http.url", urlToCall.AbsoluteUri, (TagAnnotation)r.Annotation)), Is.True.After(DelayInMilliseconds, PollingInterval));
            Assert.That(records.Any(r => r.Annotation is TagAnnotation && has("http.path", urlToCall.AbsolutePath, (TagAnnotation)r.Annotation)), Is.True.After(DelayInMilliseconds, PollingInterval));
        }

        [Test]
        public async Task Check_That_dispatcher_is_called_with_expected_records_on_POST_call()
        {
            //Arrange
            var urlToCall = new Uri("http://testserver/api/values");
            var serviceName = "OwinTest";

            Func<HttpClient, Task> clientCall = async (client) =>
            {
                var response = await client.PostAsync(urlToCall, new StringContent(""));
                var result = await response.Content.ReadAsStringAsync();
            };

            //Act
            await Call(DefaultStartup(serviceName, _traceExtractor), clientCall);

            //Assert
            Trace trace = null;
            _traceExtractor.ReceivedWithAnyArgs(1).TryExtract(Arg.Any<IHeaderDictionary>(), Arg.Any<Func<IHeaderDictionary, string, string>>(), out trace);

            var records = _tracer.Records;
            if (!IsRunningOnMono)
            {
                Assert.That(records.Any(r => r.Annotation is ServerRecv), Is.True.After(DelayInMilliseconds, PollingInterval));
                Assert.That(records.Any(r => r.Annotation is ServerSend), Is.True.After(DelayInMilliseconds, PollingInterval));
                Assert.That(records.Any(r => r.Annotation is Rpc && ((Rpc)r.Annotation).Name == "POST"), Is.True.After(DelayInMilliseconds, PollingInterval));
                Assert.That(records.Any(r => r.Annotation is ServiceName && ((ServiceName)r.Annotation).Service == serviceName), Is.True.After(DelayInMilliseconds, PollingInterval));
            }
            Assert.That(records.Any(r => r.Annotation is TagAnnotation && has("http.host", urlToCall.Host, (TagAnnotation)r.Annotation)), Is.True.After(DelayInMilliseconds, PollingInterval));
            Assert.That(records.Any(r => r.Annotation is TagAnnotation && has("http.url", urlToCall.AbsoluteUri, (TagAnnotation)r.Annotation)), Is.True.After(DelayInMilliseconds, PollingInterval));
            Assert.That(records.Any(r => r.Annotation is TagAnnotation && has("http.path", urlToCall.AbsolutePath, (TagAnnotation)r.Annotation)), Is.True.After(DelayInMilliseconds, PollingInterval));
        }
    }
}
