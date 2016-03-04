using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Criteo.Profiling.Tracing.Tracers.Zipkin.Thrift;
using Thrift.Protocol;
using Thrift.Transport;

namespace Criteo.Profiling.Tracing.Tracers.Zipkin
{
    /// <summary>
    /// Represent the creation and handling of a single RPC request.
    /// </summary>
    internal class Span
    {
        public SpanState SpanState { get; private set; }

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
            get { return !SpanState.ParentSpanId.HasValue; }
        }

        /// <summary>
        /// DateTime of the span creation. It is currently only use for flushing old spans.
        /// </summary>
        public DateTime Started { get; private set; }

        internal const string DefaultRpcMethod = "UnknownRpc";

        public Span(SpanState spanState, DateTime started)
        {
            Annotations = new List<ZipkinAnnotation>();
            BinaryAnnotations = new List<BinaryAnnotation>();
            Complete = false;
            SpanState = spanState;
            Started = started;
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
                Id = SpanState.SpanId,
                Trace_id = SpanState.TraceId,
                Name = Name ?? DefaultRpcMethod,
                Debug = false
            };

            if (!IsRoot)
            {
                thriftSpan.Parent_id = SpanState.ParentSpanId;
            }

            // Use default value if no information were recorded
            if (Endpoint == null) Endpoint = TraceManager.Configuration.DefaultEndPoint;
            if (String.IsNullOrWhiteSpace(ServiceName)) ServiceName = TraceManager.Configuration.DefaultServiceName;

            ServiceName = ServiceName.Replace(" ", "_"); // whitespaces cause issues with the query and ui

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

        /// <summary>
        /// Thrift serialize to memory stream
        /// </summary>
        /// <param name="stream"></param>
        public void SerializeToMemory(MemoryStream stream)
        {
            var thriftSpan = ToThrift();

            var transport = new TStreamTransport(null, stream);
            var protocol = new TBinaryProtocol(transport);

            thriftSpan.Write(protocol);
        }

        public override string ToString()
        {
            return String.Format("Span: {0} name={1} Annotations={2} BinAnnotations={3}", SpanState, Name, ToString(Annotations, ","), ToString(BinaryAnnotations, ","));
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
