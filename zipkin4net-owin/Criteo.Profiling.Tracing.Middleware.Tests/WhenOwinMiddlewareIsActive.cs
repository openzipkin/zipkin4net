using Criteo.Profiling.Tracing.Annotation;
using Criteo.Profiling.Tracing.Dispatcher;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Criteo.Profiling.Tracing.Middleware.Tests.Helpers;
using Criteo.Profiling.Tracing.Transport;
using Microsoft.Owin;

namespace Criteo.Profiling.Tracing.Middleware.Tests
{
    using static OwinHelper;
    using static CheckHelper;

    public class WhenOwinMiddlewareIsActive
    {
        IList<Record> records;
        ILogger logger;
        ITracer tracer;
        IRecordDispatcher dispatcher;
        ITraceExtractor traceExtractor;

        [SetUp]
        public void Setup()
        {
            logger = Substitute.For<ILogger>();
            tracer = Substitute.For<ITracer>();
            dispatcher = Substitute.For<IRecordDispatcher>();
            traceExtractor = Substitute.For<ITraceExtractor>();

            records = new List<Record>();
            dispatcher.Dispatch(Arg.Do<Record>(r => records.Add(r)));

            TraceManager.SamplingRate = 1.0f;
            TraceManager.RegisterTracer(tracer);
            TraceManager.Start(logger, dispatcher);
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
            await Call(DefaultStartup(serviceName, traceExtractor), clientCall);

            //Assert
            Trace trace = null;
            traceExtractor.ReceivedWithAnyArgs(1).TryExtract(Arg.Any<IHeaderDictionary>(), Arg.Any<Func<IHeaderDictionary, string, string>>(), out trace);
            Assert.True(records.Any(r => r.Annotation is ServerRecv));
            Assert.True(records.Any(r => r.Annotation is ServerSend));
            Assert.True(records.Any(r => r.Annotation is TagAnnotation && has("http.host", urlToCall.Host, (TagAnnotation)r.Annotation)));
            Assert.True(records.Any(r => r.Annotation is TagAnnotation && has("http.url", urlToCall.AbsoluteUri, (TagAnnotation)r.Annotation)));
            Assert.True(records.Any(r => r.Annotation is TagAnnotation && has("http.path", urlToCall.AbsolutePath, (TagAnnotation)r.Annotation)));
            Assert.True(records.Any(r => r.Annotation is Rpc && ((Rpc)r.Annotation).Name == "GET"));
            Assert.True(records.Any(r => r.Annotation is ServiceName && ((ServiceName)r.Annotation).Service == serviceName));
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
            await Call(DefaultStartup(serviceName, traceExtractor), clientCall);

            //Assert
            Trace trace = null;
            traceExtractor.ReceivedWithAnyArgs(1).TryExtract(Arg.Any<IHeaderDictionary>(), Arg.Any<Func<IHeaderDictionary, string, string>>(), out trace);
            Assert.True(records.Any(r => r.Annotation is ServerRecv));
            Assert.True(records.Any(r => r.Annotation is ServerSend));
            Assert.True(records.Any(r => r.Annotation is TagAnnotation && has("http.host", urlToCall.Host, (TagAnnotation)r.Annotation)));
            Assert.True(records.Any(r => r.Annotation is TagAnnotation && has("http.url", urlToCall.AbsoluteUri, (TagAnnotation)r.Annotation)));
            Assert.True(records.Any(r => r.Annotation is TagAnnotation && has("http.path", urlToCall.AbsolutePath, (TagAnnotation)r.Annotation)));
            Assert.True(records.Any(r => r.Annotation is Rpc && ((Rpc)r.Annotation).Name == "POST"));
            Assert.True(records.Any(r => r.Annotation is ServiceName && ((ServiceName)r.Annotation).Service == serviceName));
        }
    }
}
