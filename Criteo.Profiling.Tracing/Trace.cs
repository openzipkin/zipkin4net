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
        public SpanState CurrentSpan { get; private set; }

        private Guid? _correlationId;

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
        /// Creates a trace from an existing span state.
        /// </summary>
        /// <param name="spanState"></param>
        /// <returns></returns>
        public static Trace CreateFromId(SpanState spanState)
        {
            return new Trace(spanState);
        }

        private Trace(SpanState spanState)
        {
            CurrentSpan = new SpanState(spanState.TraceId, spanState.ParentSpanId, spanState.SpanId, spanState.Flags);
        }

        private Trace(long traceId)
        {
            CurrentSpan = CreateRootSpanId(traceId);
        }

        private static SpanState CreateRootSpanId(long traceId)
        {
            return new SpanState(traceId: traceId, parentSpanId: null, spanId: RandomUtils.NextLong(), flags: Flags.Empty);
        }

        /// <summary>
        /// Returns the trace id. It represents the correlation id
        /// of a request through the platform.
        /// For now it is based on the CurrentSpan.TraceId which is only 8 bytes instead of 16.
        /// </summary>
        public Guid CorrelationId
        {
            get
            {
                if (_correlationId == null)
                {
                    _correlationId = NumberUtils.LongToGuid(CurrentSpan.TraceId);
                }
                return _correlationId.Value;
            }
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
            CurrentSpan.SetSampled();
        }

        private SpanState CreateChildSpanId()
        {
            return new SpanState(traceId: CurrentSpan.TraceId, parentSpanId: CurrentSpan.SpanId, spanId: RandomUtils.NextLong(), flags: CurrentSpan.Flags);
        }

        internal void RecordAnnotation(IAnnotation annotation)
        {
            var record = new Record(CurrentSpan, TimeUtils.UtcNow, annotation);
            TraceManager.Dispatcher.Dispatch(record);
        }

        public bool Equals(Trace other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(CurrentSpan, other.CurrentSpan);
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
            return (CurrentSpan != null ? CurrentSpan.GetHashCode() : 0);
        }

        public override string ToString()
        {
            return String.Format("Trace [{0}]", CurrentSpan);
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
