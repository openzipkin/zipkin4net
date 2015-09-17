using System;
using System.Collections.Generic;
using System.Net;
using Criteo.Profiling.Tracing.Annotation;
using Criteo.Profiling.Tracing.Logger;
using Criteo.Profiling.Tracing.Utils;

namespace Criteo.Profiling.Tracing
{

    /// <summary>
    /// Represents a trace. It records the annotations to the globally registered tracers.
    /// </summary>
    public sealed class Trace : IEquatable<Trace>
    {

        internal SpanId CurrentId { get; private set; }

        private static IPEndPoint defaultEndPoint = new IPEndPoint(IpUtils.GetLocalIpAddress() ?? IPAddress.Loopback, 0);

        private static string defaultServiceName = "Unknown Service";

        /// <summary>
        /// Basic logger to record events. By default NO-OP logger.
        /// </summary>
        private static ILogger logger = new VoidLogger();

        public static void SetLogger(ILogger l)
        {
            logger = l;
        }

        /// <summary>
        /// Default endpoint to use if none was recorded using an annotation.
        /// </summary>
        public static IPEndPoint DefaultEndPoint
        {
            get { return defaultEndPoint; }
            set { defaultEndPoint = value; }
        }

        /// <summary>
        /// Default service/application name if none was recorded using an annotation.
        /// </summary>
        public static string DefaultServiceName
        {
            get { return defaultServiceName; }
            set { defaultServiceName = value; }
        }

        /// <summary>
        /// Globally set the state of the tracing. Annotations are ignored when set to false.
        /// </summary>
        public static bool TracingEnabled { get; set; }

        /// <summary>
        /// Starts a new trace with a random id, no parent and empty flags.
        /// </summary>
        /// <returns></returns>
        public static Trace Create()
        {
            return new Trace();
        }

        /// <summary>
        /// Creates a trace from an existing span id.
        /// </summary>
        /// <param name="spanId"></param>
        /// <returns></returns>
        public static Trace CreateFromId(SpanId spanId)
        {
            return new Trace(spanId);
        }

        private Trace()
        {
            CurrentId = NextId();
        }

        private Trace(SpanId spanId)
        {
            CurrentId = new SpanId(spanId.TraceId, spanId.ParentSpanId, spanId.Id, spanId.Flags.Copy());
        }

        private SpanId NextId()
        {
            var spanId = RandomUtils.NextLong();
            return (CurrentId == null) ? new SpanId(RandomUtils.NextLong(), 0, spanId, flags: null) : new SpanId(CurrentId.TraceId, CurrentId.Id, spanId, CurrentId.Flags.Copy());
        }

        /// <summary>
        /// Creates a derived trace which inherits from
        /// the trace id and flags.
        /// It has a new span id and the parent id set to the current span id.
        /// </summary>
        /// <returns></returns>
        public Trace Child()
        {
            return new Trace(NextId());
        }

        public void Record(IAnnotation annotation)
        {
            if (!TracingEnabled) return;

            var utcNow = DateTime.UtcNow;

            foreach (var tracer in Tracer.Tracers)
            {
                try
                {
                    tracer.Record(new Record(CurrentId, utcNow, annotation, 0));
                }
                catch (Exception ex)
                {
                    // No exception coming for traces should disrupt the main application as tracing is optional.
                    logger.LogWarning("An error occured while recording the annotation. Msg: " + ex.Message);
                }
            }
        }

        public void Record(IEnumerable<IAnnotation> annotations)
        {
            foreach (var annotation in annotations)
            {
                Record(annotation);
            }
        }

        public bool Equals(Trace other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(CurrentId, other.CurrentId);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            var objTrace = obj as Trace;
            return objTrace != null && Equals(objTrace);
        }

        public override int GetHashCode()
        {
            return (CurrentId != null ? CurrentId.GetHashCode() : 0);
        }

        public override string ToString()
        {
            return String.Format("Trace [{0}]", CurrentId);
        }

    }
}
