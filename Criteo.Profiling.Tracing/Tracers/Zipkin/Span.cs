using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Criteo.Profiling.Tracing.Tracers.Zipkin.Thrift;

namespace Criteo.Profiling.Tracing.Tracers.Zipkin
{
    /// <summary>
    /// Represent the creation and handling of a single RPC request.
    /// </summary>
    internal class Span
    {

        public SpanId SpanId { get; private set; }

        public ICollection<ZipkinAnnotation> Annotations { get; private set; }

        public ICollection<BinaryAnnotation> BinaryAnnotations { get; private set; }

        /// <summary>
        /// Local endpoint on which this span was created
        /// </summary>
        public IPEndPoint Endpoint { get; set; }

        /// <summary>
        /// Name of the service handling the request (e.g. arbitrage, cas, ...)
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// Name of the RPC method (e.g. get, post, ...)
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Whether this span is considered complete and can be logged.
        /// </summary>
        public bool Complete { get; private set; }

        /// <summary>
        /// True if this span doesn't have a parent.
        /// </summary>
        public bool IsRoot
        {
            get { return !SpanId.ParentSpanId.HasValue; }
        }

        /// <summary>
        /// DateTime of the span creation. It is currently only use for flushing old spans.
        /// </summary>
        public DateTime Started { get; private set; }

        internal const string DefaultRpcMethod = "UnknownRpc";

        public Span(SpanId spanId, DateTime started)
        {
            this.Annotations = new List<ZipkinAnnotation>();
            this.BinaryAnnotations = new List<BinaryAnnotation>();
            this.Complete = false;
            this.SpanId = spanId;
            this.Started = started;
        }

        public void AddAnnotation(ZipkinAnnotation annotation)
        {
            if (annotation.Value == zipkinCoreConstants.CLIENT_RECV ||
                annotation.Value == zipkinCoreConstants.SERVER_SEND)
            {
                Complete = true;
            }

            Annotations.Add(annotation);
        }

        public void AddBinaryAnnotation(BinaryAnnotation binaryAnnotation)
        {
            BinaryAnnotations.Add(binaryAnnotation);
        }

        /// <summary>
        /// Convert this span object to its Thrift equivalent.
        /// </summary>
        /// <returns></returns>
        public Thrift.Span ToThrift()
        {
            var thriftSpan = new Thrift.Span()
            {
                Id = SpanId.Id,
                Trace_id = SpanId.TraceId,
                Name = Name ?? DefaultRpcMethod,
                Debug = false
            };

            if (!IsRoot)
            {
                thriftSpan.Parent_id = SpanId.ParentSpanId;
            }

            // Use default value if no information were recorded
            if (Endpoint == null) Endpoint = Trace.DefaultEndPoint;
            if (String.IsNullOrWhiteSpace(ServiceName)) ServiceName = Trace.DefaultServiceName;

            var host = new Endpoint()
            {
                Ipv4 = IpToInt(Endpoint.Address),
                Port = (short)Endpoint.Port,
                Service_name = ServiceName
            };

            foreach (var ann in Annotations)
            {
                var annThrift = ann.ToThrift();

                annThrift.Host = host;

                if (thriftSpan.Annotations == null)
                    thriftSpan.Annotations = new List<Thrift.Annotation>();

                thriftSpan.Annotations.Add(annThrift);
            }

            foreach (var binaryAnnotation in BinaryAnnotations)
            {
                var annBinThrift = binaryAnnotation.ToThrift();

                annBinThrift.Host = host;

                if (thriftSpan.Binary_annotations == null)
                    thriftSpan.Binary_annotations = new List<Thrift.BinaryAnnotation>();

                thriftSpan.Binary_annotations.Add(annBinThrift);
            }

            return thriftSpan;
        }

        public override string ToString()
        {
            return String.Format("Span: {0} name={1} Annotations={2} BinAnnotations={3}", SpanId, Name, ToString(Annotations, ","), ToString(BinaryAnnotations, ","));
        }

        private static string ToString<T>(IEnumerable<T> l, string separator)
        {
            return "[" + String.Join(separator, l.Select(i => i.ToString()).ToArray()) + "]";
        }

        internal static int IpToInt(IPAddress ipAddr)
        {
            // GetAddressBytes() returns in network order (big-endian)
            return IPAddress.NetworkToHostOrder(BitConverter.ToInt32(ipAddr.GetAddressBytes(), 0));
        }

    }
}
