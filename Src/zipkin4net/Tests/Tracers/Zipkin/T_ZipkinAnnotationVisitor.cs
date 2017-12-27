using System;
using System.Linq;
using System.Net;
using zipkin4net.Annotation;
using zipkin4net.Tracers.Zipkin;
using zipkin4net.Tracers.Zipkin.Thrift;
using zipkin4net.Utils;
using NUnit.Framework;
using Span = zipkin4net.Tracers.Zipkin.Span;

namespace zipkin4net.UTest.Tracers.Zipkin
{
    [TestFixture]
    internal class T_ZipkinAnnotationVisitor
    {

        private static readonly SpanState SpanState = new SpanState(1, 0, 2, isSampled: null, isDebug: false);

        [Test]
        public void RpcNameAnnotationChangesSpanName()
        {
            var span = new Span(SpanState, spanCreated: TimeUtils.UtcNow);

            CreateAndVisitRecord(span, Annotations.Rpc("myRPCmethod"));

            Assert.AreEqual("myRPCmethod", span.Name);
        }

        [Test]
        public void ServNameAnnotationChangesSpanServName()
        {
            var span = new Span(SpanState, spanCreated: TimeUtils.UtcNow);

            CreateAndVisitRecord(span, Annotations.ServiceName("myService"));

            Assert.AreEqual("myService", span.ServiceName);
        }

        [Test]
        public void LocalAddrAnnotationChangesSpanLocalAddr()
        {
            var span = new Span(SpanState, spanCreated: TimeUtils.UtcNow);

            var ipEndpoint = new IPEndPoint(IPAddress.Loopback, 9987);
            CreateAndVisitRecord(span, Annotations.LocalAddr(ipEndpoint));

            Assert.AreEqual(ipEndpoint, span.Endpoint);
        }

        [Test]
        [Description("RPC, ServiceName and LocalAddr annotations override any previous recorded value.")]
        public void LastAnnotationValueIsKeptIfMultipleRecord()
        {
            var span = new Span(SpanState, spanCreated: TimeUtils.UtcNow);

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
            var span = new Span(SpanState, spanCreated: TimeUtils.UtcNow);

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
            var span = new Span(SpanState, spanCreated: TimeUtils.UtcNow);

            var record = new Record(SpanState, TimeUtils.UtcNow, new TagAnnotation("magicKey", 1f));
            var visitor = new ZipkinAnnotationVisitor(record, span);

            Assert.Throws<ArgumentException>(() => record.Annotation.Accept(visitor));
        }

        [Test]
        [Description("Span should only be marked as complete when either ClientRecv, ServerSend or LocalOperationStop are present.")]
        public void SimpleAnnotationsCorrectlyAdded()
        {
            AnnotationCorrectlyAdded(Annotations.ClientSend(), zipkinCoreConstants.CLIENT_SEND, false, false);
            AnnotationCorrectlyAdded(Annotations.ClientRecv(), zipkinCoreConstants.CLIENT_RECV, false, true);
            AnnotationCorrectlyAdded(Annotations.ServerRecv(), zipkinCoreConstants.SERVER_RECV, false, false);
            AnnotationCorrectlyAdded(Annotations.ServerSend(), zipkinCoreConstants.SERVER_SEND, false, true);
            AnnotationCorrectlyAdded(Annotations.WireRecv(), zipkinCoreConstants.WIRE_RECV, false, false);
            AnnotationCorrectlyAdded(Annotations.WireSend(), zipkinCoreConstants.WIRE_SEND, false, false);
            AnnotationCorrectlyAdded(Annotations.LocalOperationStart("Operation"), zipkinCoreConstants.LOCAL_COMPONENT, true, false);
            AnnotationCorrectlyAdded(Annotations.ConsumerStart(), zipkinCoreConstants.MESSAGE_RECV, false, false);
            AnnotationCorrectlyAdded(Annotations.ProducerStart(), zipkinCoreConstants.MESSAGE_SEND, false, false);
            AnnotationCorrectlyAdded(Annotations.MessageAddr("service", new IPEndPoint(0, 1)), zipkinCoreConstants.MESSAGE_ADDR, true, false);
        }

        private static void AnnotationCorrectlyAdded(IAnnotation ann, string expectedValue, bool isBinaryAnnotation, bool spanCompleted)
        {
            var span = new Span(SpanState, spanCreated: TimeUtils.UtcNow);

            var record = new Record(SpanState, TimeUtils.UtcNow, ann);
            var visitor = new ZipkinAnnotationVisitor(record, span);

            Assert.AreEqual(0, span.Annotations.Count);
            Assert.AreEqual(0, span.BinaryAnnotations.Count);

            record.Annotation.Accept(visitor);

            if (isBinaryAnnotation)
            {
                Assert.AreEqual(0, span.Annotations.Count);
                Assert.AreEqual(1, span.BinaryAnnotations.Count);
                Assert.AreEqual(expectedValue, span.BinaryAnnotations.First(_ => true).Key);
            }
            else
            {
                Assert.AreEqual(1, span.Annotations.Count);
                Assert.AreEqual(0, span.BinaryAnnotations.Count);
                Assert.AreEqual(expectedValue, span.Annotations.First(_ => true).Value);
            }

            Assert.AreEqual(spanCompleted, span.Complete);
        }
    }
}
