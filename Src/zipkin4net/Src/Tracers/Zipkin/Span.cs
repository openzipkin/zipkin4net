using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Resources;
using zipkin4net.Tracers.Zipkin.Thrift;

namespace zipkin4net.Tracers.Zipkin
{
    /// <summary>
    /// Represent the creation and handling of a single RPC request
    /// or of a single local component.
    /// </summary>
    public class Span
    {
        public ITraceContext SpanState { get; private set; }
        
        public SpanKind? SpanKind { get; set; }

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
        public DateTime SpanCreated { get; private set; }

        /// <summary>
        /// DateTime of the first operation of this span (ServerRecv, ClientSend, or LocalComponent).
        /// It is computed when a span is completed.
        /// </summary>
        public DateTime? SpanStarted { get; private set; }

        /// <summary>
        /// Duration of the span. It is computed when a span is completed.
        /// </summary>
        public TimeSpan? Duration { get; private set; }

        private const double MinimumDuration = 0.001;


        public Span(ITraceContext spanState, DateTime spanCreated)
        {
            Annotations = new List<ZipkinAnnotation>();
            BinaryAnnotations = new List<BinaryAnnotation>();
            Complete = false;
            SpanState = spanState;
            SpanCreated = spanCreated;
        }

        public void AddAnnotation(ZipkinAnnotation annotation)
        {
            Annotations.Add(annotation);
        }

        public void AddBinaryAnnotation(BinaryAnnotation binaryAnnotation)
        {
            BinaryAnnotations.Add(binaryAnnotation);
        }

        public void SetAsComplete(DateTime timestamp)
        {
            Complete = true;
            SetDurationAndSpanStarted(timestamp);
        }

        private void SetDurationAndSpanStarted(DateTime endTime)
        {
            var startTime = default(DateTime);
            BinaryAnnotation binaryAnnotation;
            ZipkinAnnotation annotation;

            // Priority is for local component duration
            if (TryGetLocalComponent(out binaryAnnotation))
            {
                startTime = binaryAnnotation.Timestamp;
            }
            // Else for the client duration
            else if (TryGetClientSend(out annotation))
            {
                startTime = annotation.Timestamp;
            }
            // Else look for server annotations
            else if(IsRoot && TryGetServerRecv(out annotation))
            {
                startTime = annotation.Timestamp;
            }

            if (startTime == default(DateTime))
                return;

            SpanStarted = startTime;
            TimeSpan? duration = endTime.Subtract(startTime);
            var durationValue = duration.Value.TotalMilliseconds;

            if (durationValue <= 0)
            {
                Duration = null;
            }
            else
            {
                Duration = durationValue < MinimumDuration ? TimeSpan.FromTicks(TimeSpan.TicksPerMillisecond / 1000) : duration;
            }
        }

        private bool TryGetLocalComponent(out BinaryAnnotation localComponentBinAnnotation)
        {
            localComponentBinAnnotation = BinaryAnnotations.FirstOrDefault(a => a.Key.Equals(zipkinCoreConstants.LOCAL_COMPONENT));
            return localComponentBinAnnotation != default(BinaryAnnotation);
        }

        private bool TryGetClientSend(out ZipkinAnnotation clientSendAnnotation)
        {
            return TryGetAnnotation(zipkinCoreConstants.CLIENT_SEND, out clientSendAnnotation);
        }

        private bool TryGetServerRecv(out ZipkinAnnotation serverRecvAnnotation)
        {
            return TryGetAnnotation(zipkinCoreConstants.SERVER_RECV, out serverRecvAnnotation);
        }

        private bool TryGetAnnotation(string annotationType, out ZipkinAnnotation annotation)
        {
            annotation = Annotations.FirstOrDefault(a => a.Value.Equals(annotationType));
            return annotation != default(ZipkinAnnotation);
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
