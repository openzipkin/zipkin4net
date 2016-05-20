using System;
using System.Linq;
using System.Net;
using Criteo.Profiling.Tracing.Annotation;
using Criteo.Profiling.Tracing.Tracers.Zipkin;
using Criteo.Profiling.Tracing.Tracers.Zipkin.Thrift;
using Criteo.Profiling.Tracing.Utils;
using NUnit.Framework;
using Span = Criteo.Profiling.Tracing.Tracers.Zipkin.Span;

namespace Criteo.Profiling.Tracing.UTest.Tracers.Zipkin
{
    [TestFixture]
    internal class T_ZipkinAnnotationVisitor
    {

        private static readonly SpanState SpanState = new SpanState(1, 0, 2, SpanFlags.None);

        [Test]
        public void RpcNameAnnotationChangesSpanName()
        {
            var span = new Span(SpanState, started: TimeUtils.UtcNow);

            CreateAndVisitRecord(span, Annotations.Rpc("myRPCmethod"));

            Assert.AreEqual("myRPCmethod", span.Name);
        }

        [Test]
        public void ServNameAnnotationChangesSpanServName()
        {
            var span = new Span(SpanState, started: TimeUtils.UtcNow);

            CreateAndVisitRecord(span, Annotations.ServiceName("myService"));

            Assert.AreEqual("myService", span.ServiceName);
        }

        [Test]
        public void LocalAddrAnnotationChangesSpanLocalAddr()
        {
            var span = new Span(SpanState, started: TimeUtils.UtcNow);

            var ipEndpoint = new IPEndPoint(IPAddress.Loopback, 9987);
            CreateAndVisitRecord(span, Annotations.LocalAddr(ipEndpoint));

            Assert.AreEqual(ipEndpoint, span.Endpoint);
        }

        [Test]
        [Description("RPC, ServiceName and LocalAddr annotations override any previous recorded value.")]
        public void LastAnnotationValueIsKeptIfMultipleRecord()
        {
            var span = new Span(SpanState, started: TimeUtils.UtcNow);

            CreateAndVisitRecord(span, Annotations.ServiceName("myService"));
            CreateAndVisitRecord(span, Annotations.ServiceName("someOtherName"));

            Assert.AreEqual("someOtherName", span.ServiceName);
        }

        private static void CreateAndVisitRecord(Span span, IAnnotation annotation)
        {
            var record = new Record(span.SpanState, TimeUtils.UtcNow, annotation);
            var visitor = new ZipkinAnnotationVisitor(record, span);

            record.Annotation.Accept(visitor);
        }

        [TestCase("string", new byte[] { 0x73, 0x74, 0x72, 0x69, 0x6E, 0x67 }, AnnotationType.STRING)]
        [TestCase(true, new byte[] { 0x1 }, AnnotationType.BOOL)]
        [TestCase(short.MaxValue, new byte[] { 0x7F, 0xFF }, AnnotationType.I16)]
        [TestCase(int.MaxValue, new byte[] { 0x7F, 0xFF, 0xFF, 0xFF }, AnnotationType.I32)]
        [TestCase(long.MaxValue, new byte[] { 0x7F, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }, AnnotationType.I64)]
        [TestCase(new byte[] { 0x93 }, new byte[] { 0x93 }, AnnotationType.BYTES)]
        [TestCase(9.3d, new byte[] { 0x40, 0x22, 0x99, 0x99, 0x99, 0x99, 0x99, 0x9A, }, AnnotationType.DOUBLE)]
        public void TagAnnotationCorrectlyAdded(object value, byte[] expectedBytes, AnnotationType expectedType)
        {
            var span = new Span(SpanState, started: TimeUtils.UtcNow);

            var record = new Record(SpanState, TimeUtils.UtcNow, new TagAnnotation("magicKey", value));
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
        public void UnsupportedTagAnnotationShouldThrow()
        {
            var span = new Span(SpanState, started: TimeUtils.UtcNow);

            var record = new Record(SpanState, TimeUtils.UtcNow, new TagAnnotation("magicKey", 1f));
            var visitor = new ZipkinAnnotationVisitor(record, span);

            Assert.Throws<ArgumentException>(() => record.Annotation.Accept(visitor));
        }

        [Test]
        public void SimpleAnnotationsCorrectlyAdded()
        {
            AnnotationCorrectlyAdded(Annotations.ClientSend(), zipkinCoreConstants.CLIENT_SEND);
            AnnotationCorrectlyAdded(Annotations.ClientRecv(), zipkinCoreConstants.CLIENT_RECV);
            AnnotationCorrectlyAdded(Annotations.ServerRecv(), zipkinCoreConstants.SERVER_RECV);
            AnnotationCorrectlyAdded(Annotations.ServerSend(), zipkinCoreConstants.SERVER_SEND);
        }


        private static void AnnotationCorrectlyAdded(IAnnotation ann, string expectedValue)
        {
            var span = new Span(SpanState, started: TimeUtils.UtcNow);

            var record = new Record(SpanState, TimeUtils.UtcNow, ann);
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
