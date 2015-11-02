using System;
using System.Net;
using Criteo.Profiling.Tracing.Tracers.Zipkin;
using Criteo.Profiling.Tracing.Tracers.Zipkin.Thrift;
using NUnit.Framework;
using Span = Criteo.Profiling.Tracing.Tracers.Zipkin.Span;

namespace Criteo.Profiling.Tracing.UTest.Tracers.Zipkin
{
    [TestFixture]
    class T_Span
    {
        [Test]
        [Description("Span should only be marked as complete when either ClientRecv or ServerSend are present.")]
        public void SpansAreLabeledAsCompleteWhenCrOrSs()
        {
            var traceId = new SpanId(1, 0, 2, Flags.Empty());
            var started = DateTime.UtcNow;

            var spanClientRecv = new Span(traceId, started);
            Assert.False(spanClientRecv.Complete);

            spanClientRecv.AddAnnotation(new ZipkinAnnotation(DateTime.UtcNow, zipkinCoreConstants.CLIENT_RECV));
            Assert.True(spanClientRecv.Complete);

            var spanServSend = new Span(traceId, started);
            Assert.False(spanServSend.Complete);

            spanServSend.AddAnnotation(new ZipkinAnnotation(DateTime.UtcNow, zipkinCoreConstants.SERVER_SEND));
            Assert.True(spanServSend.Complete);


            var spanOtherAnn = new Span(traceId, started);
            Assert.False(spanOtherAnn.Complete);

            spanServSend.AddAnnotation(new ZipkinAnnotation(DateTime.UtcNow, zipkinCoreConstants.SERVER_RECV));
            spanServSend.AddAnnotation(new ZipkinAnnotation(DateTime.UtcNow, zipkinCoreConstants.CLIENT_SEND));
            Assert.False(spanOtherAnn.Complete);
        }

        [Test]
        public void SpanCorrectlyConvertedToThrift()
        {

            var hostIp = IPAddress.Loopback;
            const int hostPort = 1234;
            const string serviceName = "myCriteoService";
            const string methodName = "GET";

            var traceId = new SpanId(1, 0, 2, Flags.Empty());
            var started = DateTime.UtcNow;
            var span = new Span(traceId, started) { Endpoint = new IPEndPoint(hostIp, hostPort), ServiceName = serviceName, Name = methodName };

            var zipkinAnnDateTime = DateTime.UtcNow;
            span.AddAnnotation(new ZipkinAnnotation(zipkinAnnDateTime, zipkinCoreConstants.CLIENT_SEND));
            span.AddAnnotation(new ZipkinAnnotation(zipkinAnnDateTime, zipkinCoreConstants.CLIENT_RECV));

            const string binAnnKey = "http.uri";
            var binAnnVal = new byte[] { 0x00 };
            const AnnotationType binAnnType = AnnotationType.STRING;

            span.AddBinaryAnnotation(new Tracing.Tracers.Zipkin.BinaryAnnotation(binAnnKey, binAnnVal, binAnnType));

            var thriftSpan = span.ToThrift();

            var expectedHost = new Endpoint()
            {
                Ipv4 = Span.IpToInt(hostIp),
                Port = hostPort,
                Service_name = serviceName
            };

            Assert.AreEqual(1, thriftSpan.Trace_id);
            Assert.AreEqual(0, thriftSpan.Parent_id);
            Assert.AreEqual(2, thriftSpan.Id);
            Assert.AreEqual(false, thriftSpan.Debug);
            Assert.AreEqual(methodName, thriftSpan.Name);

            Assert.AreEqual(2, thriftSpan.Annotations.Count);

            thriftSpan.Annotations.ForEach(ann =>
            {
                Assert.AreEqual(expectedHost, ann.Host);
                Assert.IsNull(ann.Duration);
                Assert.AreEqual(ZipkinAnnotation.ToUnixTimestamp(zipkinAnnDateTime), ann.Timestamp);

            });

            Assert.AreEqual(1, thriftSpan.Binary_annotations.Count);

            thriftSpan.Binary_annotations.ForEach(ann =>
            {
                Assert.AreEqual(expectedHost, ann.Host);
                Assert.AreEqual(binAnnKey, ann.Key);
                Assert.AreEqual(binAnnVal, ann.Value);
                Assert.AreEqual(binAnnType, ann.Annotation_type);
            });
        }

        [Test]
        public void DefaultsValuesAreUsedIfNothingSpecified()
        {
            var traceId = new SpanId(1, 0, 2, Flags.Empty());
            var started = DateTime.UtcNow;
            var span = new Span(traceId, started);

            span.AddAnnotation(new ZipkinAnnotation(DateTime.UtcNow, zipkinCoreConstants.CLIENT_SEND));
            span.AddAnnotation(new ZipkinAnnotation(DateTime.UtcNow, zipkinCoreConstants.CLIENT_RECV));

            var thriftSpan = span.ToThrift();

            Assert.NotNull(thriftSpan);
            Assert.AreEqual(2, thriftSpan.Annotations.Count);

            thriftSpan.Annotations.ForEach(ann =>
            {
                Assert.AreEqual(Trace.DefaultServiceName, ann.Host.Service_name);
                Assert.AreEqual(Span.IpToInt(Trace.DefaultEndPoint.Address), ann.Host.Ipv4);
                Assert.AreEqual(Trace.DefaultEndPoint.Port, ann.Host.Port);
            });
        }

        [Test]
        public void DefaultsValuesAreNotUsedIfValuesSpecified()
        {
            var traceId = new SpanId(1, 0, 2, Flags.Empty());
            var started = DateTime.UtcNow;

            // Make sure we choose something different thant the default values
            var serviceName = Trace.DefaultServiceName + "_notDefault";
            var hostPort = Trace.DefaultEndPoint.Port + 1;

            var span = new Span(traceId, started) { Endpoint = new IPEndPoint(IPAddress.Loopback, hostPort), ServiceName = serviceName };

            span.AddAnnotation(new ZipkinAnnotation(DateTime.UtcNow, zipkinCoreConstants.CLIENT_SEND));
            span.AddAnnotation(new ZipkinAnnotation(DateTime.UtcNow, zipkinCoreConstants.CLIENT_RECV));

            var thriftSpan = span.ToThrift();

            Assert.NotNull(thriftSpan);
            Assert.AreEqual(2, thriftSpan.Annotations.Count);

            thriftSpan.Annotations.ForEach(annotation =>
            {
                Assert.AreEqual(serviceName, annotation.Host.Service_name);
                Assert.AreEqual(Span.IpToInt(IPAddress.Loopback), annotation.Host.Ipv4);
                Assert.AreEqual(hostPort, annotation.Host.Port);
            });
        }

        [Test]
        public void IpToIntConversionIsCorrect()
        {
            const string ipStr = "192.168.1.56";
            const int expectedIp = unchecked((int)3232235832);

            var ipAddr = IPAddress.Parse(ipStr);

            var ipInt = Span.IpToInt(ipAddr);

            Assert.AreEqual(expectedIp, ipInt);
        }


    }
}
