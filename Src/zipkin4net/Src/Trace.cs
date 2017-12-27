using System;
using zipkin4net.Annotation;
using zipkin4net.Utils;

namespace zipkin4net
{

    /// <summary>
    /// Represents a trace. It records the annotations to the globally registered tracers.
    /// </summary>
    public sealed partial class Trace : IEquatable<Trace>
    {
        /// <summary>
        /// Experimental feature, do not use.
        /// </summary>
        public static Trace Current
        {
            get
            {
                return TraceContext.Get();
            }
            set
            {
                if (value == null)
                {
                    TraceContext.Clear();
                }
                else
                {
                    TraceContext.Set(value);
                }
            }
        }

        public ITraceContext CurrentSpan { get; private set; }

        /// <summary>
        /// Returns the trace id. It represents the correlation id
        /// of a request through the platform.
        /// For now it is based on the CurrentSpan.TraceId which is only 8 bytes instead of 16.
        /// </summary>
        public Guid CorrelationId { get; private set; }

        /// <summary>
        /// Starts a new trace with a random id, no parent and empty flags.
        /// </summary>
        /// <returns></returns>
        public static Trace Create()
        {
            return new Trace();
        }
        private Trace()
        {
            var traceId = RandomUtils.NextLong();
            var traceIdHigh = TraceManager.Trace128Bits ? RandomUtils.NextLong() : 0;
            var spanId = RandomUtils.NextLong();

            var isSampled = TraceManager.Sampler.Sample(traceId);

            CurrentSpan = new SpanState(traceIdHigh: traceIdHigh, traceId: traceId, parentSpanId: null, spanId: spanId, isSampled: isSampled, isDebug: false);
            CorrelationId = NumberUtils.LongToGuid(traceId);
        }

        /// <summary>
        /// Creates a trace from an existing span state.
        /// </summary>
        /// <param name="spanState"></param>
        /// <returns></returns>
        public static Trace CreateFromId(ITraceContext spanState)
        {
            return new Trace(spanState);
        }

        private Trace(ITraceContext spanState)
        {
            CurrentSpan = spanState;
            CorrelationId = NumberUtils.LongToGuid(CurrentSpan.TraceId);
        }

        /// <summary>
        /// Creates a derived trace which inherits from
        /// the trace id and flags.
        /// It has a new span id and the parent id set to the current span id.
        /// </summary>
        /// <returns></returns>
        public Trace Child()
        {
            var childState = new SpanState(traceIdHigh: CurrentSpan.TraceIdHigh, traceId: CurrentSpan.TraceId, parentSpanId: CurrentSpan.SpanId, spanId: RandomUtils.NextLong(), isSampled: CurrentSpan.Sampled, isDebug: CurrentSpan.Debug);
            return new Trace(childState);
        }

        public bool IsSampled => ShouldBeRecorded();

        /// <summary>
        /// Force this trace to be sent.
        /// </summary>
        public void ForceSampled()
        {
            CurrentSpan = new SpanState(traceIdHigh: CurrentSpan.TraceIdHigh, traceId: CurrentSpan.TraceId, parentSpanId: CurrentSpan.SpanId, spanId: RandomUtils.NextLong(), isSampled: true, isDebug: CurrentSpan.Debug);
        }

        internal void RecordAnnotation(IAnnotation annotation)
        {
            RecordAnnotation(annotation, TimeUtils.UtcNow);
        }

        internal void RecordAnnotation(IAnnotation annotation, DateTime recordTime)
        {
            if (ShouldBeRecorded())
            {
                TraceManager.Dispatch(new Record(CurrentSpan, recordTime, annotation));
            }
        }

        public bool Equals(Trace other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return CorrelationId.Equals(other.CorrelationId) && Equals(CurrentSpan, other.CurrentSpan);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is Trace && Equals((Trace)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (CorrelationId.GetHashCode() * 397) ^ (CurrentSpan != null ? CurrentSpan.GetHashCode() : 0);
            }
        }

        public override string ToString()
        {
            return string.Format("Trace [{0}] (CorrelationId {1})", CurrentSpan, CorrelationId.ToString("D"));
        }

        private bool ShouldBeRecorded()
        {
            return CurrentSpan.Sampled ?? false;
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

        public static void Record(this Trace trace, IAnnotation annotation, DateTime recordTime)
        {
            if (trace != null)
            {
                trace.RecordAnnotation(annotation, recordTime);
            }
        }
    }
}
