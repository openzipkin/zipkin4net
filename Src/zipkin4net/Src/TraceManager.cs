using System;
using System.Collections.Generic;
using System.Threading;
using zipkin4net.Dispatcher;
using zipkin4net.Logger;
using zipkin4net.Sampling;
using zipkin4net.Utils;

namespace zipkin4net
{
    public static class TraceManager
    {
        private static int _status = (int)Status.Stopped;

        private static readonly TimeSpan MinimumTimeBetweenDispatchFailureLogs = TimeSpan.FromMinutes(1);
        private static IRecordDispatcher _dispatcher = new VoidDispatcher();
        private static DateTime _lastLoggedDispatchFailureMessage = default(DateTime);

        internal static readonly ISampler Sampler = new DefaultSampler(salt: RandomUtils.NextLong(), samplingRate: 0f);
        internal static ILogger Logger = new VoidLogger();

        /// <summary>
        /// Global list of registred tracers.
        /// </summary>
        private static ICollection<ITracer> _tracers = new List<ITracer>();

        internal static ICollection<ITracer> Tracers
        {
            get { return _tracers; }
        }

        /// <summary>
        /// Sampling of the tracing. Between 0.0 (not tracing) and 1.0 (full tracing). Default 0.0
        /// </summary>
        public static float SamplingRate
        {
            get { return Sampler.SamplingRate; }
            set { Sampler.SamplingRate = value; }
        }

        /// <summary>
        /// Returns true if tracing is currently running and forwarding records to the registered tracers.
        /// </summary>
        /// <returns></returns>
        public static bool Started
        {
            get { return _status == (int)Status.Started; }
        }

        public static bool Trace128Bits
        {
            get; set;
        }

        /// <summary>
        /// Start tracing, records will be forwarded to the registered tracers.
        /// </summary>
        /// <returns>True if successfully started, false if error or the service was already running.</returns>
        public static bool Start(ILogger logger)
        {
            return Start(logger, new InOrderAsyncQueueDispatcher(Push));
        }

        internal static bool Start(ILogger logger, IRecordDispatcher dispatcher)
        {
            if (Interlocked.CompareExchange(ref _status, (int)Status.Started, (int)Status.Stopped) ==
                      (int)Status.Stopped)
            {
                Logger = logger;
                _dispatcher = dispatcher;
                Logger.LogInformation("Tracing dispatcher started");
                Logger.LogInformation("HighResolutionDateTime is " + (HighResolutionDateTime.IsAvailable ? "available" : "not available"));
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
                _dispatcher.Stop();
                _dispatcher = new VoidDispatcher();
                Logger.LogInformation("Tracing dispatcher stopped");
                return true;
            }

            return false;
        }

        /// <summary>
        /// Adds a new tracer. Subsequent annotation records will be forwarded to the tracer.
        /// </summary>
        /// <param name="tracer"></param>
        public static void RegisterTracer(ITracer tracer)
        {
            var tracers = new List<ITracer>(_tracers) { tracer };

            _tracers = tracers;
        }

        /// <summary>
        /// Clears the registered tracers.
        /// </summary>
        public static void ClearTracers()
        {
            _tracers = new List<ITracer>();
        }

        internal static void Dispatch(Record record)
        {
            if (!_dispatcher.Dispatch(record))
            {
                var utcNow = TimeUtils.UtcNow;
                if (ShouldLogDispatchFailure(utcNow)) // not thread safe we can possibly log multiples warn at the same instant
                {
                    Logger.LogWarning("Couldn't dispatch record, actor may be blocked by another operation");
                    _lastLoggedDispatchFailureMessage = utcNow;
                }
            }
        }

        private static bool ShouldLogDispatchFailure(DateTime now)
        {
            return _lastLoggedDispatchFailureMessage == default(DateTime) || now.Subtract(_lastLoggedDispatchFailureMessage) > MinimumTimeBetweenDispatchFailureLogs;
        }

        internal static void Push(Record record)
        {
            foreach (var tracer in _tracers)
            {
                try
                {
                    tracer.Record(record);
                }
                catch (Exception ex)
                {
                    // No exception coming for traces should disrupt the main application as tracing is optional.
                    Logger.LogWarning("An error occured while recording the annotation: " + ex);
                }
            }
        }

        private enum Status
        {
            Started,
            Stopped
        }
    }
}
