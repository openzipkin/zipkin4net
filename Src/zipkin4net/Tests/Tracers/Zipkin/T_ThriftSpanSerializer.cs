using System;
using System.Linq;
using System.Net;
using System.Text;
using zipkin4net.Tracers.Zipkin;
using zipkin4net.Tracers.Zipkin.Thrift;
using zipkin4net.Utils;
using NUnit.Framework;
using BinaryAnnotation = zipkin4net.Tracers.Zipkin.BinaryAnnotation;
using Span = zipkin4net.Tracers.Zipkin.Span;

namespace zipkin4net.UTest.Tracers.Zipkin
{

    [TestFixture]
    internal class T_ThriftSpanSerializer
    {
        private const string SomeRandomAnnotation = "SomethingHappenedHere";

        private readonly Endpoint _someHost = new Endpoint { Service_name = "myService", Port = 80, Ipv4 = 123456 };

        [Test]
        public void ThriftConversionBinaryAnnotationIsCorrect()
        {
            string serviceName = null;
            IPEndPoint ipEndPoint = null;
            AssertBinaryAnnotationConversion(serviceName, ipEndPoint, _someHost);
        }

        [Test]
        public void ThriftConversionBinaryAnnotationWithEndPointIsCorrect()
        {
            var serviceName = "database";
            var ipEndPoint = new IPEndPoint(1, 2);
            var expectedEndpoint = new Endpoint { Service_name = serviceName, Ipv4 = SerializerUtils.IpToInt(ipEndPoint.Address), Port = (short)ipEndPoint.Port };
            AssertBinaryAnnotationConversion(serviceName, ipEndPoint, expectedEndpoint);
        }

        [Test]
        public void ThriftConversionBinaryAnnotationWithEndPointButNoServiceNameIsCorrect()
        {
            string serviceName = null;
            var ipEndPoint = new IPEndPoint(1, 2);
            var expectedEndpoint = new Endpoint { Service_name = _someHost.Service_name, Ipv4 = SerializerUtils.IpToInt(ipEndPoint.Address), Port = (short)ipEndPoint.Port };
            AssertBinaryAnnotationConversion(serviceName, ipEndPoint, expectedEndpoint);
        }

        private void AssertBinaryAnnotationConversion(string serviceName, IPEndPoint endpoint, Endpoint expectedEndpoint)
        {
            const string key = "myKey";
            var data = Encoding.ASCII.GetBytes("hello");
            const AnnotationType type = AnnotationType.STRING;

            var binAnn = new BinaryAnnotation(key, data, type, TimeUtils.UtcNow, serviceName, endpoint);

            var thriftBinAnn = ThriftSpanSerializer.ConvertToThrift(binAnn, _someHost);

            Assert.AreEqual(key, thriftBinAnn.Key);
            Assert.AreEqual(data, thriftBinAnn.Value);
            Assert.AreEqual(type, thriftBinAnn.Annotation_type);
            AssertEndpointIsEqual(expectedEndpoint,  thriftBinAnn.Host);
        }

        [Test]
        public void ThriftConversionLocalComponentWithHostAndEmptyServiceName()
        {
            var binAnn = new BinaryAnnotation(zipkinCoreConstants.LOCAL_COMPONENT, Encoding.ASCII.GetBytes("hello"), AnnotationType.STRING, TimeUtils.UtcNow, null, null);
            var thriftBinAnn = ThriftSpanSerializer.ConvertToThrift(binAnn, _someHost);
            AssertEndpointIsCorrect(thriftBinAnn.Host);
        }

        [Test]
        public void ThriftConversionZipkinAnnotationIsCorrect()
        {
            var now = TimeUtils.UtcNow;
            const string value = "anything";
            var ann = new ZipkinAnnotation(now, value);

            var thriftAnn = ThriftSpanSerializer.ConvertToThrift(ann, _someHost);

            Assert.NotNull(thriftAnn);
            Assert.AreEqual(now.ToUnixTimestamp(), thriftAnn.Timestamp);
            Assert.AreEqual(value, thriftAnn.Value);
            AssertEndpointIsCorrect(thriftAnn.Host);
        }

        private void AssertEndpointIsEqual(Endpoint expectedEndPoint, Endpoint endpoint)
        {
            Assert.AreEqual(expectedEndPoint.Service_name, endpoint.Service_name);
            Assert.AreEqual(expectedEndPoint.Port, endpoint.Port);
            Assert.AreEqual(expectedEndPoint.Ipv4, endpoint.Ipv4);
        }

        private void AssertEndpointIsCorrect(Endpoint endpoint)
        {
            AssertEndpointIsEqual(_someHost, endpoint);
        }

        [TestCase(null)]
        [TestCase(123456L)]
        public void SpanCorrectlyConvertedToThrift(long? parentSpanId)
        {
            var hostIp = IPAddress.Loopback;
            const int hostPort = 1234;
            const string serviceName = "myCriteoService";
            const string methodName = "GET";

            var spanState = new SpanState(2, 1, parentSpanId, 2, isSampled: null, isDebug: false);
            var timestamp = TimeUtils.UtcNow;
            var span = new Span(spanState, timestamp) { Endpoint = new IPEndPoint(hostIp, hostPort), ServiceName = serviceName, Name = methodName };

            var zipkinAnnDateTime = TimeUtils.UtcNow;
            var timeOffset = TimeSpan.FromMilliseconds(500);
            AddClientSendReceiveAnnotations(span, zipkinAnnDateTime, timeOffset);
            span.AddAnnotation(new ZipkinAnnotation(zipkinAnnDateTime, SomeRandomAnnotation));

            const string binAnnKey = "http.uri";
            var binAnnVal = new byte[] { 0x00 };
            const AnnotationType binAnnType = AnnotationType.STRING;

            span.AddBinaryAnnotation(new BinaryAnnotation(binAnnKey, binAnnVal, binAnnType, TimeUtils.UtcNow, null, null));

            var thriftSpan = ThriftSpanSerializer.ConvertToThrift(span);

            var expectedHost = new Endpoint()
            {
                Ipv4 = SerializerUtils.IpToInt(hostIp),
                Port = hostPort,
                Service_name = serviceName
            };

            Assert.AreEqual(spanState.TraceIdHigh, thriftSpan.Trace_id_high);
            Assert.AreEqual(spanState.TraceId, thriftSpan.Trace_id);
            Assert.AreEqual(spanState.SpanId, thriftSpan.Id);
            Assert.True(thriftSpan.Timestamp.HasValue);

            if (span.IsRoot)
            {
                Assert.IsNull(thriftSpan.Parent_id); // root span has no parent
            }
            else
            {
                Assert.AreEqual(parentSpanId, thriftSpan.Parent_id);
            }

            Assert.AreEqual(false, thriftSpan.Debug);
            Assert.AreEqual(methodName, thriftSpan.Name);

            Assert.AreEqual(3, thriftSpan.Annotations.Count);

            thriftSpan.Annotations.ForEach(ann =>
            {
                Assert.AreEqual(expectedHost, ann.Host);
            });

            Assert.AreEqual(thriftSpan.Annotations.FirstOrDefault(a => a.Value.Equals(zipkinCoreConstants.CLIENT_SEND)).Timestamp, zipkinAnnDateTime.ToUnixTimestamp());
            Assert.AreEqual(thriftSpan.Annotations.FirstOrDefault(a => a.Value.Equals(zipkinCoreConstants.CLIENT_RECV)).Timestamp, (zipkinAnnDateTime + timeOffset).ToUnixTimestamp());
            Assert.AreEqual(thriftSpan.Annotations.FirstOrDefault(a => a.Value.Equals(SomeRandomAnnotation)).Timestamp, zipkinAnnDateTime.ToUnixTimestamp());

            Assert.AreEqual(1, thriftSpan.Binary_annotations.Count);

            thriftSpan.Binary_annotations.ForEach(ann =>
            {
                Assert.AreEqual(expectedHost, ann.Host);
                Assert.AreEqual(binAnnKey, ann.Key);
                Assert.AreEqual(binAnnVal, ann.Value);
                Assert.AreEqual(binAnnType, ann.Annotation_type);
            });

            Assert.AreEqual(thriftSpan.Duration, timeOffset.TotalMilliseconds * 1000);
        }

        [Test]
        public void TimestampConvertedForLocalComponent()
        {
            var startTime = DateTime.Now;
            var spanState = new SpanState(1, 0, 2, isSampled: null, isDebug: false);
            var span = new Span(spanState, TimeUtils.UtcNow);

            span.AddBinaryAnnotation(new BinaryAnnotation(zipkinCoreConstants.LOCAL_COMPONENT, Encoding.UTF8.GetBytes("lc"), AnnotationType.STRING, startTime, null, SerializerUtils.DefaultEndPoint));
            span.SetAsComplete(startTime.AddHours(1));
            var thriftSpan = ThriftSpanSerializer.ConvertToThrift(span);
            Assert.AreEqual(startTime.ToUnixTimestamp(), thriftSpan.Timestamp);
            Assert.AreEqual(1, thriftSpan.Binary_annotations.Count);
            var endpoint = thriftSpan.Binary_annotations[0].Host;
            Assert.NotNull(endpoint);
            Assert.IsEmpty(endpoint.Service_name);
            Assert.IsNotNull(endpoint.Ipv4);
        }

        [Test]
        [Description("Span should never be sent without required fields such as Name, ServiceName, Ipv4 or Port")]
        public void DefaultsValuesAreUsedIfNothingSpecified()
        {
            var spanState = new SpanState(1, 0, 2, isSampled: null, isDebug: false);
            var span = new Span(spanState, TimeUtils.UtcNow);
            AddClientSendReceiveAnnotations(span);

            var thriftSpan = ThriftSpanSerializer.ConvertToThrift(span);
            AssertSpanHasRequiredFields(thriftSpan);

            const string defaultName = SerializerUtils.DefaultRpcMethodName;
            const string defaultServiceName = SerializerUtils.DefaultServiceName;
            var defaultIpv4 = SerializerUtils.IpToInt(SerializerUtils.DefaultEndPoint.Address);
            var defaultPort = SerializerUtils.DefaultEndPoint.Port;

            Assert.AreEqual(2, thriftSpan.Annotations.Count);
            thriftSpan.Annotations.ForEach(ann =>
            {
                Assert.AreEqual(defaultServiceName, ann.Host.Service_name);
                Assert.AreEqual(defaultIpv4, ann.Host.Ipv4);
                Assert.AreEqual(defaultPort, ann.Host.Port);
            });

            Assert.AreEqual(defaultName, thriftSpan.Name);
            Assert.IsNull(thriftSpan.Duration);
        }

        [Test]
        public void DefaultsValuesAreNotUsedIfValuesSpecified()
        {
            var spanState = new SpanState(1, 0, 2, isSampled: null, isDebug: false);
            var started = TimeUtils.UtcNow;

            // Make sure we choose something different thant the default values
            const string serviceName = SerializerUtils.DefaultServiceName + "_notDefault";
            var hostPort = SerializerUtils.DefaultEndPoint.Port + 1;

            const string name = "myRPCmethod";

            var span = new Span(spanState, started) { Endpoint = new IPEndPoint(IPAddress.Loopback, hostPort), ServiceName = serviceName, Name = name };
            AddClientSendReceiveAnnotations(span);

            var thriftSpan = ThriftSpanSerializer.ConvertToThrift(span);
            AssertSpanHasRequiredFields(thriftSpan);

            Assert.NotNull(thriftSpan);
            Assert.AreEqual(2, thriftSpan.Annotations.Count);

            thriftSpan.Annotations.ForEach(annotation =>
            {
                Assert.AreEqual(serviceName, annotation.Host.Service_name);
                Assert.AreEqual(SerializerUtils.IpToInt(IPAddress.Loopback), annotation.Host.Ipv4);
                Assert.AreEqual(hostPort, annotation.Host.Port);
            });

            Assert.AreEqual(name, thriftSpan.Name);
        }

        [TestCase(null)]
        [TestCase(123456L)]
        public void RootSpanPropertyIsCorrect(long? parentSpanId)
        {
            var spanState = new SpanState(1, parentSpanId, 1, isSampled: null, isDebug: false);
            var span = new Span(spanState, TimeUtils.UtcNow);

            Assert.AreEqual(parentSpanId == null, span.IsRoot);
        }

        [Test]
        public void WhiteSpacesAreRemovedFromServiceName()
        {
            var spanState = new SpanState(1, 0, 2, isSampled: null, isDebug: false);
            var span = new Span(spanState, TimeUtils.UtcNow) { ServiceName = "my Criteo Service" };
            AddClientSendReceiveAnnotations(span);

            var thriftSpan = ThriftSpanSerializer.ConvertToThrift(span);

            Assert.AreEqual("my_Criteo_Service", thriftSpan.Annotations[0].Host.Service_name);
        }

        private static void AddClientSendReceiveAnnotations(Span span)
        {
            AddClientSendReceiveAnnotations(span, TimeUtils.UtcNow, new TimeSpan(0));
        }

        private static void AddClientSendReceiveAnnotations(Span span, DateTime startTime, TimeSpan timeOffset )
        {
            var endtime = startTime + timeOffset;
            span.AddAnnotation(new ZipkinAnnotation(startTime, zipkinCoreConstants.CLIENT_SEND));
            span.AddAnnotation(new ZipkinAnnotation(endtime, zipkinCoreConstants.CLIENT_RECV));
            span.SetAsComplete(endtime);
        }

        private static void AssertSpanHasRequiredFields(zipkin4net.Tracers.Zipkin.Thrift.Span thriftSpan)
        {
            Assert.IsNotNull(thriftSpan.Id);
            Assert.IsNotNull(thriftSpan.Trace_id);
            Assert.False(string.IsNullOrEmpty(thriftSpan.Name));

            thriftSpan.Annotations.ForEach(annotation =>
            {
                Assert.False(string.IsNullOrEmpty(annotation.Host.Service_name));
                Assert.IsNotNull(annotation.Host.Ipv4);
                Assert.IsNotNull(annotation.Host.Port);

                Assert.IsNotNull(annotation.Timestamp);
                Assert.That(annotation.Timestamp, Is.GreaterThan(0));
                Assert.False(string.IsNullOrEmpty(annotation.Value));
            });

            if (thriftSpan.Binary_annotations != null)
            {
                thriftSpan.Binary_annotations.ForEach(annotation =>
                {
                    Assert.False(string.IsNullOrEmpty(annotation.Host.Service_name));
                    Assert.IsNotNull(annotation.Host.Ipv4);
                    Assert.IsNotNull(annotation.Host.Port);

                    Assert.IsNotNull(annotation.Annotation_type);
                    Assert.IsNotNull(annotation.Value);
                });
            }
        }

    }
}
