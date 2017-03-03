using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Criteo.Profiling.Tracing.Tracers.Zipkin.Thrift;
using Criteo.Profiling.Tracing.Utils;
using Thrift.Protocol;
using Thrift.Transport;

namespace Criteo.Profiling.Tracing.Tracers.Zipkin
{
    internal class ThriftSpanSerializer : ISpanSerializer
    {

        /// <summary>
        /// Name of the RPC method when none has been recorded
        /// </summary>
        public const string DefaultRpcMethodName = "UnknownRpc";

        /// <summary>
        /// Name of the service when none has been recorded
        /// </summary>
        public const string DefaultServiceName = "UnknownService";

        /// <summary>
        /// IpEndpoint to use when none has been recorded
        /// </summary>
        public static readonly IPEndPoint DefaultEndPoint = new IPEndPoint(IpUtils.GetLocalIpAddress() ?? IPAddress.Loopback, 0);

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
                Trace_id = span.SpanState.TraceId,
                Name = span.Name ?? DefaultRpcMethodName,
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
            var spanEndpoint = span.Endpoint ?? DefaultEndPoint;
            var spanServiceName = GetServiceNameOrDefault(span);
            spanServiceName = spanServiceName.Replace(" ", "_"); // whitespaces cause issues with the query and ui

            var host = new Endpoint
            {
                Ipv4 = IpToInt(spanEndpoint.Address),
                Port = (short)spanEndpoint.Port,
                Service_name = spanServiceName
            };

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

        private static string GetServiceNameOrDefault(Span span)
        {
            if (string.IsNullOrWhiteSpace(span.ServiceName))
            {
                // Since we don't have the app name yet, we need to hack a bit by providing
                // an empty service name. This will add the endpoint attribute, and thus enable
                // clock skew correction on the server.
                return IsLocalSpan(span) ? string.Empty : DefaultServiceName;
            }
            return span.ServiceName;
        }

        private static bool IsLocalSpan(Span span)
        {
            return !span.Annotations.Any() &&
                span.BinaryAnnotations.Any(ba => ba.Key == zipkinCoreConstants.LOCAL_COMPONENT);
        }

        /// <summary>
        /// Convert this annotation object to its Thrift equivalent.
        /// </summary>
        /// <returns></returns>
        public static Thrift.Annotation ConvertToThrift(ZipkinAnnotation zipkinAnnotation, Endpoint host)
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
        public static Thrift.BinaryAnnotation ConvertToThrift(BinaryAnnotation binaryAnnotation, Endpoint host)
        {
            return new Thrift.BinaryAnnotation
            {
                Annotation_type = binaryAnnotation.AnnotationType,
                Key = binaryAnnotation.Key,
                Value = binaryAnnotation.Value,
                Host = host
            };
        }

        public static int IpToInt(IPAddress ipAddr)
        {
            // GetAddressBytes() returns in network order (big-endian)
            return IPAddress.NetworkToHostOrder(BitConverter.ToInt32(ipAddr.GetAddressBytes(), 0));
        }

    }
}
