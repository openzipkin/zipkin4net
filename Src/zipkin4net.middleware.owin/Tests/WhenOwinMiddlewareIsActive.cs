using zipkin4net.Annotation;
using zipkin4net.Tracers;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
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
        private InMemoryTracer _tracer;

        [SetUp]
        public void Setup()
        {
            _logger = Substitute.For<ILogger>();
            _tracer = new InMemoryTracer();

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

            Func<HttpClient, Task<string>> clientCall = async (client) =>
            {
                var response = await client.GetAsync(urlToCall);
                return await response.Content.ReadAsStringAsync();
            };

            //Act
            await Call(DefaultStartup(serviceName), clientCall);

            var records = _tracer.Records;

            if (!IsRunningOnMono)
            {
                AssertAnnotationReceived<ServerRecv>(records);
                AssertAnnotationReceived<ServerSend>(records);
                AssertAnnotationReceived<Rpc>(records, rpc => rpc.Name == "GET");
                AssertAnnotationReceived<ServiceName>(records, serviceNameAnnotation => serviceNameAnnotation.Service == serviceName);
            }
            AssertAnnotationReceived<TagAnnotation>(records, tag => has("http.host", urlToCall.Host, tag));
            AssertAnnotationReceived<TagAnnotation>(records, tag => has("http.url", urlToCall.AbsoluteUri, tag));
            AssertAnnotationReceived<TagAnnotation>(records, tag => has("http.path", urlToCall.AbsolutePath, tag));
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

            var records = _tracer.Records;
            if (!IsRunningOnMono)
            {
                AssertAnnotationReceived<ServerRecv>(records);
                AssertAnnotationReceived<ServerSend>(records);
                AssertAnnotationReceived<Rpc>(records, rpc => rpc.Name == "POST");
                AssertAnnotationReceived<ServiceName>(records, serviceNameAnnotation => serviceNameAnnotation.Service == serviceName);
            }
            AssertAnnotationReceived<TagAnnotation>(records, tag => has("http.host", urlToCall.Host, tag));
            AssertAnnotationReceived<TagAnnotation>(records, tag => has("http.url", urlToCall.AbsoluteUri, tag));
            AssertAnnotationReceived<TagAnnotation>(records, tag => has("http.path", urlToCall.AbsolutePath, tag));
        }

        private static void AssertAnnotationReceived<T>(IEnumerable<Record> records)
        {
            AssertAnnotationReceived<T>(records, annotation => true);
        }

        private static void AssertAnnotationReceived<T>(IEnumerable<Record> records, Func<T, bool> assertionOnAnnotation)
        {
            Assert.That(records.Any(r => r.Annotation is T && assertionOnAnnotation((T) r.Annotation)), Is.True.After(DelayInMilliseconds, PollingInterval),
                $"Didn't get {typeof(T)} annotation within 5s. Annotations: [{string.Join(", ", records)}]");
        }
    }
}
