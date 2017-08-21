using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using zipkin4net.Tracers.Zipkin.Thrift;
using zipkin4net.Utils;
using Thrift.Protocol;
using Thrift.Transport;

namespace zipkin4net.Tracers.Zipkin
{
    public class ThriftSpanSerializer : ISpanSerializer
    {
        public void SerializeTo(Stream stream, Span span)
        {
            var thriftSpan = ConvertToThrift(span);

            var transport = new TStreamTransport(null, stream);
            var protocol = new TBinaryProtocol(transport);

            protocol.WriteListBegin(new TList(TType.Struct, 1));
            thriftSpan.Write(protocol);
            protocol.WriteListEnd();
        }

        /// <summary>
        /// Convert this span object to its Thrift equivalent.
        /// </summary>
        /// <returns></returns>
        public static Thrift.Span ConvertToThrift(Span span)
        {
            var thriftSpan = new Thrift.Span()
            {
                Id = span.SpanState.SpanId,
                Trace_id_high = span.SpanState.TraceIdHigh,
                Trace_id = span.SpanState.TraceId,
                Name = span.Name ?? SerializerUtils.DefaultRpcMethodName,
                Debug = false
            };

            if (!span.IsRoot)
            {
                thriftSpan.Parent_id = span.SpanState.ParentSpanId;
            }

            if (span.SpanStarted.HasValue)
            {
                thriftSpan.Timestamp = span.SpanStarted.Value.ToUnixTimestamp();
            }

            // Use default value if no information were recorded
            var spanEndpoint = span.Endpoint ?? SerializerUtils.DefaultEndPoint;
            var spanServiceName = SerializerUtils.GetServiceNameOrDefault(span);

            var host = ConvertToThrift(spanEndpoint, spanServiceName);

            var thriftAnnotations = span.Annotations.Select(ann => ConvertToThrift(ann, host)).ToList();
            if (thriftAnnotations.Count > 0)
            {
                thriftSpan.Annotations = thriftAnnotations;
            }

            var thriftBinaryAnnotations = span.BinaryAnnotations.Select(ann => ConvertToThrift(ann, host)).ToList();
            if (thriftBinaryAnnotations.Count > 0)
            {
                thriftSpan.Binary_annotations = thriftBinaryAnnotations;
            }

            if (span.Duration.HasValue && span.Duration.Value.TotalMilliseconds > 0)
            {
                thriftSpan.Duration = (long)(span.Duration.Value.TotalMilliseconds * 1000); // microseconds
            }
            return thriftSpan;
        }

        /// <summary>
        /// Convert this annotation object to its Thrift equivalent.
        /// </summary>
        /// <returns></returns>
        public static Thrift.Annotation ConvertToThrift(ZipkinAnnotation zipkinAnnotation, Thrift.Endpoint host)
        {
            var thriftAnn = new Thrift.Annotation
            {
                Timestamp = zipkinAnnotation.Timestamp.ToUnixTimestamp(),
                Value = zipkinAnnotation.Value,
                Host = host
            };

            return thriftAnn;
        }

        /// <summary>
        /// Convert this span binary annotation object to its Thrift equivalent.
        /// </summary>
        /// <returns></returns>
        public static Thrift.BinaryAnnotation ConvertToThrift(BinaryAnnotation binaryAnnotation, Thrift.Endpoint spanEndpoint)
        {
            var host = spanEndpoint;
            var endpoint = binaryAnnotation.Host;
            if (endpoint != null)
            {
                host = ConvertToThrift(endpoint.IPEndPoint, endpoint.ServiceName ?? spanEndpoint.Service_name);
            }
            return new Thrift.BinaryAnnotation
            {
                Annotation_type = binaryAnnotation.AnnotationType,
                Key = binaryAnnotation.Key,
                Value = binaryAnnotation.Value,
                Host = host
            };
        }

        private static Thrift.Endpoint ConvertToThrift(IPEndPoint ipEndPoint, string serviceName)
        {
            return new Thrift.Endpoint()
            {
                Ipv4 = SerializerUtils.IpToInt(ipEndPoint.Address),
                Port = (short)ipEndPoint.Port,
                Service_name = serviceName
            };
        }
    }
}
