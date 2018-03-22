using System;
using System.Net;
using System.Text;
using NUnit.Framework;
using zipkin4net.Internal;
using zipkin4net.Tracers.Zipkin;
using zipkin4net.Tracers.Zipkin.Thrift;
using zipkin4net.Utils;
using BinaryAnnotation = zipkin4net.Tracers.Zipkin.Thrift.BinaryAnnotation;
using Span = zipkin4net.Internal.V2.Span;

namespace zipkin4net.UTest.Internal
{
    [TestFixture]
    public class T_V2SpanConverter
    {
        private const string TraceIdHigh = "7180c278b62e8f6a";
        private const string TraceId = "216a2aea45d08fc9";
        private const string ParentId = "6b221d5bc9e6496c";
        private const string SpanId = "5b4185666d50f68b";
        private const string SpanName = "get";
        private const string LocalServiceName = "frontend";
        private const string RemoteServiceName = "backend";
        private static readonly IPEndPoint LocalEndPoint = new IPEndPoint(IPAddress.Parse("10.0.0.1"), 8888);
        private static readonly IPEndPoint RemoteEndPoint = new IPEndPoint(IPAddress.Parse("10.0.0.2"), 9999);
        private static readonly DateTime Timestamp = DateTime.UtcNow;

        [Test]
        public void Client()
        {
            var v2Span = CreateV2SpanBuilder()
                .Kind(Span.SpanKind.Client)
                .LocalEndpoint(new zipkin4net.Internal.V2.Endpoint(LocalServiceName, LocalEndPoint))
                .RemoteEndpoint(new zipkin4net.Internal.V2.Endpoint(RemoteServiceName, RemoteEndPoint))
                .Timestamp(Timestamp)
                .Duration(207000L)
                .AddAnnotation(Timestamp.AddMilliseconds(4), zipkinCoreConstants.WIRE_SEND)
                .AddAnnotation(Timestamp.AddMilliseconds(21), zipkinCoreConstants.WIRE_RECV)
                .PutTag("http.path", "/api")
                .Build();

            var v1Span = CreateV1Span(Timestamp);
            AddAnnotation(v1Span, Timestamp, zipkinCoreConstants.CLIENT_SEND);
            AddAnnotation(v1Span, Timestamp.AddMilliseconds(4), zipkinCoreConstants.WIRE_SEND);
            AddAnnotation(v1Span, Timestamp.AddMilliseconds(21),zipkinCoreConstants.WIRE_RECV);
            AddAnnotation(v1Span, Timestamp.AddMilliseconds(207),zipkinCoreConstants.CLIENT_RECV);
            AddServerAddr(v1Span, Timestamp);
            AddBinaryAnnotation(v1Span, Timestamp, "http.path", Encoding.ASCII.GetBytes("/api"));

            v1Span.SetAsComplete(Timestamp.AddMilliseconds(207));
            Assert.AreEqual(v1Span, V2SpanConverter.ToSpan(v2Span));
        }

        [Test]
        public void ClientUnfinished()
        {    
            var v2Span = CreateV2SpanBuilder()
                .Kind(Span.SpanKind.Client)
                .LocalEndpoint(new zipkin4net.Internal.V2.Endpoint(LocalServiceName, LocalEndPoint))
                .Timestamp(Timestamp)
                .AddAnnotation(Timestamp.AddMilliseconds(4), zipkinCoreConstants.WIRE_SEND)
                .Build();

            var v1Span = CreateV1Span(Timestamp);
            AddAnnotation(v1Span, Timestamp, zipkinCoreConstants.CLIENT_SEND);
            AddAnnotation(v1Span, Timestamp.AddMilliseconds(4), zipkinCoreConstants.WIRE_SEND);

            Assert.AreEqual(v1Span, V2SpanConverter.ToSpan(v2Span));
        }
        
        [Test]
        public void ClientKindInferredFromAnnotation() {
            var v2Span = CreateV2SpanBuilder()
                .LocalEndpoint(new zipkin4net.Internal.V2.Endpoint(LocalServiceName, LocalEndPoint))
                .Timestamp(Timestamp)
                .Duration(207000L)
                .AddAnnotation(Timestamp, zipkinCoreConstants.CLIENT_SEND)
                .Build();

            var v1Span = CreateV1Span(Timestamp);
            AddAnnotation(v1Span, Timestamp, zipkinCoreConstants.CLIENT_SEND);
            AddAnnotation(v1Span, Timestamp.AddMilliseconds(207), zipkinCoreConstants.CLIENT_RECV);
            v1Span.SetAsComplete(Timestamp.AddMilliseconds(207));

            Assert.AreEqual(v1Span, V2SpanConverter.ToSpan(v2Span));
        }

        [Test]
        public void Server() {
            var v2Span = CreateV2SpanBuilder()
                .Kind(Span.SpanKind.Server)
                .LocalEndpoint(new zipkin4net.Internal.V2.Endpoint(LocalServiceName, LocalEndPoint))
                .RemoteEndpoint(new zipkin4net.Internal.V2.Endpoint(RemoteServiceName, RemoteEndPoint))
                .Timestamp(Timestamp)
                .Duration(207000L)
                .PutTag("http.path", "/api")
                .Build();
            
            var v1Span = CreateV1Span(Timestamp);
            AddAnnotation(v1Span, Timestamp, zipkinCoreConstants.SERVER_RECV);
            AddAnnotation(v1Span, Timestamp.AddMilliseconds(207), zipkinCoreConstants.SERVER_SEND);
            AddBinaryAnnotation(v1Span, Timestamp, zipkinCoreConstants.CLIENT_ADDR, BitConverter.GetBytes(true), RemoteServiceName, RemoteEndPoint);
            AddBinaryAnnotation(v1Span, Timestamp, "http.path", Encoding.ASCII.GetBytes("/api"));
            
            v1Span.SetAsComplete(Timestamp.AddMilliseconds(207));
            
            Assert.AreEqual(v1Span, V2SpanConverter.ToSpan(v2Span));
        }
        
        [Test]
        public void Producer()
        {
            var v2Span = CreateV2SpanBuilder()
                .Kind(Span.SpanKind.Producer)
                .LocalEndpoint(new zipkin4net.Internal.V2.Endpoint(LocalServiceName, LocalEndPoint))
                .Timestamp(Timestamp)
                .Build();

            var v1Span = CreateV1Span(Timestamp);
            AddAnnotation(v1Span, Timestamp, zipkinCoreConstants.MESSAGE_SEND);

            Assert.AreEqual(v1Span, V2SpanConverter.ToSpan(v2Span));
        }
        
        [Test]
        public void ProducerRemote()
        {
            var v2Span = CreateV2SpanBuilder()
                .Kind(Span.SpanKind.Producer)
                .LocalEndpoint(new zipkin4net.Internal.V2.Endpoint(LocalServiceName, LocalEndPoint))
                .RemoteEndpoint(new zipkin4net.Internal.V2.Endpoint(RemoteServiceName, RemoteEndPoint))
                .Timestamp(Timestamp)
                .Build();

            var v1Span = CreateV1Span(Timestamp);
                AddAnnotation(v1Span, Timestamp, zipkinCoreConstants.MESSAGE_SEND);
            AddBinaryAnnotation(v1Span, Timestamp, zipkinCoreConstants.MESSAGE_ADDR, BitConverter.GetBytes(true),
                RemoteServiceName, RemoteEndPoint);

            Assert.AreEqual(v1Span, V2SpanConverter.ToSpan(v2Span));
        }
        
        [Test]
        public void Consumer()
        {
            var v2Span = CreateV2SpanBuilder()
                .Kind(Span.SpanKind.Consumer)
                .LocalEndpoint(new zipkin4net.Internal.V2.Endpoint(LocalServiceName, LocalEndPoint))
                .Timestamp(Timestamp)
                .Build();

            var v1Span = CreateV1Span(Timestamp);
            AddAnnotation(v1Span, Timestamp, zipkinCoreConstants.MESSAGE_RECV);

            Assert.AreEqual(v1Span, V2SpanConverter.ToSpan(v2Span));
        }
        
        [Test]
        public void ConsumerRemote()
        {
            var v2Span = CreateV2SpanBuilder()
                .Kind(Span.SpanKind.Consumer)
                .LocalEndpoint(new zipkin4net.Internal.V2.Endpoint(LocalServiceName, LocalEndPoint))
                .RemoteEndpoint(new zipkin4net.Internal.V2.Endpoint(RemoteServiceName, RemoteEndPoint))
                .Timestamp(Timestamp)
                .Build();

            var v1Span = CreateV1Span(Timestamp);
            AddAnnotation(v1Span, Timestamp, zipkinCoreConstants.MESSAGE_RECV);
            AddBinaryAnnotation(v1Span, Timestamp, zipkinCoreConstants.MESSAGE_ADDR, BitConverter.GetBytes(true),
                RemoteServiceName, RemoteEndPoint);

            Assert.AreEqual(v1Span, V2SpanConverter.ToSpan(v2Span));
        }

        private static Span.Builder CreateV2SpanBuilder()
        {
            return Span.NewBuilder()
                .TraceId(TraceIdHigh + TraceId)
                .ParentId(ParentId)
                .Id(SpanId)
                .Name(SpanName);
        }
        
        private static zipkin4net.Tracers.Zipkin.Span CreateV1Span(DateTime timestamp)
        {
            var traceContext = CreateTraceContext();
            var v1Span =
                new zipkin4net.Tracers.Zipkin.Span(traceContext, timestamp)
                {
                    Endpoint = LocalEndPoint,
                    ServiceName = LocalServiceName,
                    Name = SpanName
                };
            return v1Span;
        }

        private static SpanState CreateTraceContext()
        {
            var traceContext = new SpanState(
                NumberUtils.DecodeHexString(TraceIdHigh),
                NumberUtils.DecodeHexString(TraceId),
                NumberUtils.DecodeHexString(ParentId),
                NumberUtils.DecodeHexString(SpanId),
                true,
                false);
            return traceContext;
        }
        
        private static void AddAnnotation(zipkin4net.Tracers.Zipkin.Span v1Span, DateTime timestamp, string annotation)
        {
            v1Span.AddAnnotation(new ZipkinAnnotation(timestamp, annotation));
        }

        private static void AddServerAddr(zipkin4net.Tracers.Zipkin.Span v1Span, DateTime timestamp)
        {
            AddBinaryAnnotation(v1Span, timestamp, zipkinCoreConstants.SERVER_ADDR, BitConverter.GetBytes(true), RemoteServiceName, RemoteEndPoint);
        }

        private static void AddBinaryAnnotation(zipkin4net.Tracers.Zipkin.Span v1Span, DateTime timestamp, string key,
            byte[] value)
        {
            AddBinaryAnnotation(v1Span, timestamp, key, value, LocalServiceName, LocalEndPoint);
        }

        private static void AddBinaryAnnotation(zipkin4net.Tracers.Zipkin.Span v1Span, DateTime timestamp, string key,
            byte[] value, string serviceName, IPEndPoint ipEndPoint)
        {
            v1Span.AddBinaryAnnotation(new zipkin4net.Tracers.Zipkin.BinaryAnnotation(key,
                value, AnnotationType.STRING, timestamp, serviceName, ipEndPoint));
        }
    }
}