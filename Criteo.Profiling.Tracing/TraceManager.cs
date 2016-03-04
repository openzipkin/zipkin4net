using System;
using System.Collections.Generic;
using System.Threading;
using Criteo.Profiling.Tracing.Dispatcher;
using Criteo.Profiling.Tracing.Sampling;
using Criteo.Profiling.Tracing.Utils;

namespace Criteo.Profiling.Tracing
{
    public static class TraceManager
    {
        private static int _status = (int)Status.Stopped;

        internal static readonly ISampler Sampler = new DefaultSampler(salt: RandomUtils.NextLong(), samplingRate: 0f);
        internal static Configuration Configuration = new Configuration();
        internal static IRecordDispatcher Dispatcher = new VoidDispatcher();

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

        /// <summary>
        /// Start tracing, records will be forwarded to the registered tracers.
        /// </summary>
        /// <returns>True if successfully started, false if error or the service was already running.</returns>
        public static bool Start(Configuration configuration)
        {
            return Start(configuration, new InOrderAsyncDispatcher(Push));
        }

        internal static bool Start(Configuration configuration, IRecordDispatcher dispatcher)
        {
            if (Interlocked.CompareExchange(ref _status, (int)Status.Started, (int)Status.Stopped) ==
                      (int)Status.Stopped)
            {
                Configuration = configuration;
                Dispatcher = dispatcher;
                Configuration.Logger.LogInformation("Tracing dispatcher started");
                Configuration.Logger.LogInformation("HighResolutionDateTime is " + (HighResolutionDateTime.IsAvailable ? "available" : "not available"));
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
                Dispatcher.Stop();
                Dispatcher = new VoidDispatcher();
                Configuration.Logger.LogInformation("Tracing dispatcher stopped");
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
            var tracers = new List<ITracer>(_tracers) {tracer};

            _tracers = tracers;
        }

        /// <summary>
        /// Clears the registered tracers.
        /// </summary>
        public static void ClearTracers()
        {
            _tracers = new List<ITracer>();
        }

        /// <summary>
        /// Send a record to all the registered tracers
        /// </summary>
        /// <param name="record"></param>
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
                    Configuration.Logger.LogWarning("An error occured while recording the annotation. Msg: " + ex.Message);
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
