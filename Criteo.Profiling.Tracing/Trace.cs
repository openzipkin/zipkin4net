using System;
using Criteo.Profiling.Tracing.Annotation;
using Criteo.Profiling.Tracing.Utils;

namespace Criteo.Profiling.Tracing
{

    /// <summary>
    /// Represents a trace. It records the annotations to the globally registered tracers.
    /// </summary>
    public sealed class Trace : IEquatable<Trace>
    {
        internal SpanId CurrentId { get; private set; }

        /// <summary>
        /// Starts a new trace with a random id, no parent and empty flags.
        /// </summary>
        /// <returns></returns>
        public static Trace CreateIfSampled()
        {
            var traceId = RandomUtils.NextLong();
            return TraceManager.Sampler.Sample(traceId) ? new Trace(traceId) : null;
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

        private Trace(SpanId spanId)
        {
            CurrentId = new SpanId(spanId.TraceId, spanId.ParentSpanId, spanId.Id, spanId.Flags);
        }

        private Trace(long traceId)
        {
            CurrentId = CreateRootSpanId(traceId);
        }

        private static SpanId CreateRootSpanId(long traceId)
        {
            return new SpanId(traceId: traceId, parentSpanId: null, id: RandomUtils.NextLong(), flags: Flags.Empty());
        }

        /// <summary>
        /// Returns the trace id. It represents the correlation id
        /// of a request through the platform.
        /// </summary>
        public long CorrelationId {
            get { return CurrentId.TraceId; }
        }

        /// <summary>
        /// Creates a derived trace which inherits from
        /// the trace id and flags.
        /// It has a new span id and the parent id set to the current span id.
        /// </summary>
        /// <returns></returns>
        public Trace Child()
        {
            return new Trace(CreateChildSpanId());
        }

        public void ForceSampled()
        {
            CurrentId.ForceSampled();
        }

        private SpanId CreateChildSpanId()
        {
            return new SpanId(traceId: CurrentId.TraceId, parentSpanId: CurrentId.Id, id: RandomUtils.NextLong(), flags: CurrentId.Flags);
        }

        internal void RecordAnnotation(IAnnotation annotation)
        {
            var record = new Record(CurrentId, TimeUtils.UtcNow, annotation);
            TraceManager.Dispatcher.Dispatch(record);
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

    /**
     * Traces are sampled for performance management. Therefore trace can be null
     * and you probably don't want to check for nullity every time in your code.
     */
    public static class TraceExtensions
    {
        public static void Record(this Trace trace, IAnnotation annotation)
        {
            if (trace != null)
            {
                trace.RecordAnnotation(annotation);
            }
        }
    }


}
