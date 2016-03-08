using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Criteo.Profiling.Tracing.Tracers.Zipkin.Thrift;
using Criteo.Profiling.Tracing.Utils;
using Thrift.Protocol;
using Thrift.Transport;

namespace Criteo.Profiling.Tracing.Tracers.Zipkin
{
    internal class ThriftSpanSerializer : ISpanSerializer
    {

        internal const string DefaultRpcMethod = "UnknownRpc";

        public void SerializeTo(Stream stream, Span span)
        {
            var thriftSpan = ConvertToThrift(span);

            var transport = new TStreamTransport(null, stream);
            var protocol = new TBinaryProtocol(transport);

            thriftSpan.Write(protocol);
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
                Name = span.Name ?? DefaultRpcMethod,
                Debug = false
            };

            if (!span.IsRoot)
            {
                thriftSpan.Parent_id = span.SpanState.ParentSpanId;
            }

            // Use default value if no information were recorded
            var spanEndpoint = span.Endpoint ?? TraceManager.Configuration.DefaultEndPoint;
            var spanServiceName = String.IsNullOrWhiteSpace(span.ServiceName) ? TraceManager.Configuration.DefaultServiceName : span.ServiceName;
            spanServiceName = spanServiceName.Replace(" ", "_"); // whitespaces cause issues with the query and ui

            var host = new Endpoint
            {
                Ipv4 = IpToInt(spanEndpoint.Address),
                Port = (short)spanEndpoint.Port,
                Service_name = spanServiceName
            };

            foreach (var ann in span.Annotations)
            {
                var annThrift = ConvertToThrift(ann);

                annThrift.Host = host;

                if (thriftSpan.Annotations == null)
                    thriftSpan.Annotations = new List<Thrift.Annotation>();

                thriftSpan.Annotations.Add(annThrift);
            }

            foreach (var binaryAnnotation in span.BinaryAnnotations)
            {
                var annBinThrift = ConvertToThrift(binaryAnnotation);

                annBinThrift.Host = host;

                if (thriftSpan.Binary_annotations == null)
                    thriftSpan.Binary_annotations = new List<Thrift.BinaryAnnotation>();

                thriftSpan.Binary_annotations.Add(annBinThrift);
            }

            return thriftSpan;
        }

        /// <summary>
        /// Convert this annotation object to its Thrift equivalent.
        /// </summary>
        /// <returns></returns>
        public static Thrift.Annotation ConvertToThrift(ZipkinAnnotation zipkinAnnotation)
        {
            var thriftAnn = new Thrift.Annotation
            {
                Timestamp = TimeUtils.ToUnixTimestamp(zipkinAnnotation.Timestamp),
                Value = zipkinAnnotation.Value
            };

            return thriftAnn;
        }

        /// <summary>
        /// Convert this span binary annotation object to its Thrift equivalent.
        /// </summary>
        /// <returns></returns>
        public static Thrift.BinaryAnnotation ConvertToThrift(BinaryAnnotation binaryAnnotation)
        {
            return new Thrift.BinaryAnnotation
            {
                Annotation_type = binaryAnnotation.AnnotationType,
                Key = binaryAnnotation.Key,
                Value = binaryAnnotation.Value
            };
        }

        public static int IpToInt(IPAddress ipAddr)
        {
            // GetAddressBytes() returns in network order (big-endian)
            return IPAddress.NetworkToHostOrder(BitConverter.ToInt32(ipAddr.GetAddressBytes(), 0));
        }

    }
}
