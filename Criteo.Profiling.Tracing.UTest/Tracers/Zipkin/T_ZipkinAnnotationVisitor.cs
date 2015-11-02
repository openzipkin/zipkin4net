using System;
using System.Linq;
using System.Net;
using System.Text;
using Criteo.Profiling.Tracing.Annotation;
using Criteo.Profiling.Tracing.Tracers.Zipkin;
using Criteo.Profiling.Tracing.Tracers.Zipkin.Thrift;
using NUnit.Framework;
using Span = Criteo.Profiling.Tracing.Tracers.Zipkin.Span;

namespace Criteo.Profiling.Tracing.UTest.Tracers.Zipkin
{
    [TestFixture]
    class T_ZipkinAnnotationVisitor
    {

        [Test]
        public void RpcNameAnnotationChangesSpanName()
        {
            var traceId = new SpanId(1, 0, 2, Flags.Empty());
            var span = new Span(traceId, started: DateTime.UtcNow);

            var record = new Record(traceId, DateTime.UtcNow, Annotations.Rpc("myRPCmethod"));
            var visitor = new ZipkinAnnotationVisitor(record, span);

            record.Annotation.Accept(visitor);

            Assert.AreEqual("myRPCmethod", span.Name);
        }

        [Test]
        public void ServNameAnnotationChangesSpanServName()
        {
            var traceId = new SpanId(1, 0, 2, Flags.Empty());
            var span = new Span(traceId, started: DateTime.UtcNow);

            var record = new Record(traceId, DateTime.UtcNow, Annotations.ServiceName("myService"));
            var visitor = new ZipkinAnnotationVisitor(record, span);

            record.Annotation.Accept(visitor);

            Assert.AreEqual("myService", span.ServiceName);
        }

        [Test]
        public void LocalAddrAnnotationChangesSpanLocalAddr()
        {
            var traceId = new SpanId(1, 0, 2, Flags.Empty());
            var span = new Span(traceId, started: DateTime.UtcNow);

            var ipEndpoint = new IPEndPoint(IPAddress.Loopback, 9987);
            var record = new Record(traceId, DateTime.UtcNow, Annotations.LocalAddr(ipEndpoint));
            var visitor = new ZipkinAnnotationVisitor(record, span);

            record.Annotation.Accept(visitor);

            Assert.AreEqual(ipEndpoint, span.Endpoint);
        }

        [Test]
        public void BinaryAnnotationCorrectlyAdded()
        {
            var traceId = new SpanId(1, 0, 2, Flags.Empty());
            var span = new Span(traceId, started: DateTime.UtcNow);

            var record = new Record(traceId, DateTime.UtcNow, Annotations.Binary("magicKey", "string object"));
            var visitor = new ZipkinAnnotationVisitor(record, span);

            Assert.AreEqual(0, span.Annotations.Count);
            Assert.AreEqual(0, span.BinaryAnnotations.Count);

            record.Annotation.Accept(visitor);

            Assert.AreEqual(0, span.Annotations.Count);
            Assert.AreEqual(1, span.BinaryAnnotations.Count);

            var binAnn = span.BinaryAnnotations.First(_ => true);

            Assert.AreEqual("magicKey", binAnn.Key);
            Assert.AreEqual(Encoding.ASCII.GetBytes("string object"), binAnn.Value);
            Assert.AreEqual(AnnotationType.STRING, binAnn.AnnotationType);
        }


        [Test]
        public void SimpleAnnotationsCorrectlyAdded()
        {
            AnnotationCorrectlyAdded(Annotations.ClientSend(), zipkinCoreConstants.CLIENT_SEND);
            AnnotationCorrectlyAdded(Annotations.ClientRecv(), zipkinCoreConstants.CLIENT_RECV);
            AnnotationCorrectlyAdded(Annotations.ServerRecv(), zipkinCoreConstants.SERVER_RECV);
            AnnotationCorrectlyAdded(Annotations.ServerSend(), zipkinCoreConstants.SERVER_SEND);
        }


        private static void AnnotationCorrectlyAdded(IAnnotation ann, String expectedValue)
        {
            var traceId = new SpanId(1, 0, 2, Flags.Empty());
            var span = new Span(traceId, started: DateTime.UtcNow);

            var record = new Record(traceId, DateTime.UtcNow, ann);
            var visitor = new ZipkinAnnotationVisitor(record, span);

            Assert.AreEqual(0, span.Annotations.Count);
            Assert.AreEqual(0, span.BinaryAnnotations.Count);

            record.Annotation.Accept(visitor);

            Assert.AreEqual(1, span.Annotations.Count);
            Assert.AreEqual(expectedValue, span.Annotations.First(_ => true).Value);

            Assert.AreEqual(0, span.BinaryAnnotations.Count);
        }

    }
}
