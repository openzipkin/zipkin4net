using System;
using System.Net;
using System.Threading;
using Criteo.Profiling.Tracing.Annotation;
using Criteo.Profiling.Tracing.Dispatcher;
using Criteo.Profiling.Tracing.Logger;
using Criteo.Profiling.Tracing.Sampling;
using Criteo.Profiling.Tracing.Utils;

namespace Criteo.Profiling.Tracing
{

    /// <summary>
    /// Represents a trace. It records the annotations to the globally registered tracers.
    /// </summary>
    public sealed class Trace : IEquatable<Trace>
    {

        internal SpanId CurrentId { get; private set; }

        private static IPEndPoint _defaultEndPoint = new IPEndPoint(IpUtils.GetLocalIpAddress() ?? IPAddress.Loopback, 0);
        private static string _defaultServiceName = "Unknown Service";

        private static readonly ISampler _sampler = new DefaultSampler(samplingRate: 0f);

        private static ILogger _logger = new VoidLogger();

        private static IRecordDispatcher _dispatcher = new VoidDispatcher();
        private static int _status = (int)Status.Stopped;

        /// <summary>
        /// Basic logger to record events. By default NO-OP logger.
        /// </summary>
        public static ILogger Logger
        {
            get { return _logger; }
            set { _logger = value; }
        }

        /// <summary>
        /// Default endpoint to use if none was recorded using an annotation.
        /// </summary>
        public static IPEndPoint DefaultEndPoint
        {
            get { return _defaultEndPoint; }
            set { _defaultEndPoint = value; }
        }

        /// <summary>
        /// Default service/application name if none was recorded using an annotation.
        /// </summary>
        public static string DefaultServiceName
        {
            get { return _defaultServiceName; }
            set { _defaultServiceName = value; }
        }

        /// <summary>
        /// Returns true if tracing is currently running and forwarding records to the registered tracers.
        /// </summary>
        /// <returns></returns>
        public static bool TracingRunning
        {
            get { return _status == (int)Status.Started; }
        }

        /// <summary>
        /// Start tracing, records will be forwarded to the registered tracers.
        /// </summary>
        /// <returns></returns>
        public static bool Start()
        {
            if (Interlocked.CompareExchange(ref _status, (int)Status.Started, (int)Status.Stopped) ==
                      (int)Status.Stopped)
            {
                _dispatcher = new InOrderAsyncDispatcher(PushToTracers);
                _logger.LogInformation("Tracing dispatcher service started");
                return true;
            }

            return false;
        }

        /// <summary>
        /// Stop tracing, records will be ignored.
        /// </summary>
        /// <returns></returns>
        public static bool Stop()
        {
            if (Interlocked.CompareExchange(ref _status, (int)Status.Stopped, (int)Status.Started) ==
                   (int)Status.Started)
            {
                _dispatcher.Stop(); // InOrderAsyncDispatcher
                _dispatcher = new VoidDispatcher();
                _logger.LogInformation("Tracing dispatcher service stopped");
                return true;
            }

            return false;
        }

        /// <summary>
        // Sampling of the tracing. Between 0.0 (not tracing) and 1.0 (full tracing). Default 0.0
        /// </summary>
        public static float SamplingRate
        {
            get { return _sampler.SamplingRate; }
            set { _sampler.SamplingRate = value; }
        }

        /// <summary>
        /// Starts a new trace with a random id, no parent and empty flags.
        /// </summary>
        /// <returns></returns>
        public static Trace CreateIfSampled()
        {
            var traceId = RandomUtils.NextLong();
            return _sampler.Sample(traceId) ? new Trace(traceId) : null;
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

        internal void RecordAnnotation(IAnnotation annotation)
        {
            var record = new Record(CurrentId, DateTime.UtcNow, annotation);
            _dispatcher.Dispatch(record);
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

        private enum Status
        {
            Started,
            Stopped
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
