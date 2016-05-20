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

        public override string ToString()
        {
            return string.Format("Span: {0} name={1} Annotations={2} BinAnnotations={3}", SpanState, Name, ToString(Annotations, ","), ToString(BinaryAnnotations, ","));
        }

        private static string ToString<T>(IEnumerable<T> l, string separator)
        {
            return "[" + string.Join(separator, l.Select(i => i.ToString()).ToArray()) + "]";
        }

    }
}
