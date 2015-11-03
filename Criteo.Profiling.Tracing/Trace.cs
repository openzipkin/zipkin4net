using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
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
        // Avoid that every machines sample the same traceId subset
        private static readonly long Salt = RandomUtils.NextLong();

        internal SpanId CurrentId { get; private set; }

        private static IPEndPoint defaultEndPoint = new IPEndPoint(IpUtils.GetLocalIpAddress() ?? IPAddress.Loopback, 0);

        private static string defaultServiceName = "Unknown Service";
        private static float _samplingRate = 0f;

        private static ILogger logger = new VoidLogger();

        private static bool _tracingEnabled = false;

        /// <summary>
        /// Basic logger to record events. By default NO-OP logger.
        /// </summary>
        public static ILogger Logger
        {
            get { return logger; }
            set { logger = value; }
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
        public static bool TracingEnabled
        {
            get { return _tracingEnabled; }
            set
            {
                _tracingEnabled = value;
                logger.LogInformation(string.Format("Tracing is {0}", _tracingEnabled ? "enabled" : "disabled"));
            }
        }

        /// <summary>
        // Sampling of the tracing. Between 0.0 (not tracing) and 1.0 (full tracing). Default 0.0
        /// </summary>
        public static float SamplingRate
        {
            get { return _samplingRate; }
            set
            {
                if (!IsValidSamplingRate(value))
                    throw new ArgumentOutOfRangeException("value", "Sample rate should be between 0.0 and 1.0");

                Interlocked.Exchange(ref _samplingRate, value);
            }
        }

        private static bool IsValidSamplingRate(float rate)
        {
            return 0.0f <= rate && rate <= 1.0f;
        }

        /// <summary>
        /// Starts a new trace with a random id, no parent and empty flags.
        /// </summary>
        /// <returns></returns>
        public static Trace CreateIfSampled()
        {
            var traceId = RandomUtils.NextLong();
            return RandomSample(traceId) ? new Trace(traceId) : null;
        }

        private static bool RandomSample(long id)
        {
            return Math.Abs(id ^ Salt) % 10000 < (SamplingRate * 10000);
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

        private static void PushToTracers(Record record)
        {
            foreach (var tracer in Tracer.Tracers)
            {
                try
                {
                    tracer.Record(record);
                }
                catch (Exception ex)
                {
                    // No exception coming for traces should disrupt the main application as tracing is optional.
                    Logger.LogWarning("An error occured while recording the annotation. Msg: " + ex.Message);
                }
            }
        }

        private Trace(long traceId)
        {
            CurrentId = CreateRootSpanId(traceId);
        }

        private Trace(SpanId spanId)
        {
            CurrentId = new SpanId(spanId.TraceId, spanId.ParentSpanId, spanId.Id, spanId.Flags);
        }

        private static SpanId CreateRootSpanId(long traceId)
        {
            var spanId = RandomUtils.NextLong();
            return new SpanId(traceId, 0, spanId, Flags.Empty());
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

        private SpanId CreateChildSpanId()
        {
            var spanId = RandomUtils.NextLong();
            return new SpanId(CurrentId.TraceId, CurrentId.Id, spanId, CurrentId.Flags);
        }

        internal Task RecordAnnotation(IAnnotation annotation)
        {
            var record = new Record(CurrentId, DateTime.UtcNow, annotation);
            return Task.Run(() => PushToTracers(record));
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
        public static Task Record(this Trace trace, IAnnotation annotation)
        {
            if (trace != null && Trace.TracingEnabled)
                trace.RecordAnnotation(annotation);
            return Task.FromResult(0);
        }
    }
}
