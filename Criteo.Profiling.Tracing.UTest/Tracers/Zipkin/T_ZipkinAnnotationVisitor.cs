using System;
using System.Linq;
using System.Net;
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

        [TestCase("string", new byte[] { 0x73, 0x74, 0x72, 0x69, 0x6E, 0x67 }, AnnotationType.STRING)]
        [TestCase(true, new byte[] { 0x1 }, AnnotationType.BOOL)]
        [TestCase(Int16.MaxValue, new byte[] { 0xFF, 0x7F }, AnnotationType.I16)]
        [TestCase(Int32.MaxValue, new byte[] { 0xFF, 0xFF, 0xFF, 0x7F }, AnnotationType.I32)]
        [TestCase(Int64.MaxValue, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x7F }, AnnotationType.I64)]
        [TestCase(new byte[] { 0x93 }, new byte[] { 0x93 }, AnnotationType.BYTES)]
        [TestCase(9.3d, new byte[] { 0x9A, 0x99, 0x99, 0x99, 0x99, 0x99, 0x22, 0x40 }, AnnotationType.DOUBLE)]
        public void BinaryAnnotationCorrectlyAdded(object value, byte[] expectedBytes, AnnotationType expectedType)
        {
            var traceId = new SpanId(1, 0, 2, Flags.Empty());
            var span = new Span(traceId, started: DateTime.UtcNow);

            var record = new Record(traceId, DateTime.UtcNow, Annotations.Binary("magicKey", value));
            var visitor = new ZipkinAnnotationVisitor(record, span);

            Assert.AreEqual(0, span.Annotations.Count);
            Assert.AreEqual(0, span.BinaryAnnotations.Count);

            record.Annotation.Accept(visitor);

            Assert.AreEqual(0, span.Annotations.Count);
            Assert.AreEqual(1, span.BinaryAnnotations.Count);

            var binAnn = span.BinaryAnnotations.First(_ => true);

            Assert.AreEqual("magicKey", binAnn.Key);
            Assert.AreEqual(expectedBytes, binAnn.Value);
            Assert.AreEqual(expectedType, binAnn.AnnotationType);
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
