using System;
using System.Diagnostics;

namespace zipkin4net.Tracers.Zipkin
{
    /// <summary>
    /// The RateLimiterZipkinSender throttle the trace sending via a token bucket.
    /// </summary>
    public class RateLimiterZipkinSender : IZipkinSender
    {
        private readonly object _lockObject = new object();
        private readonly IZipkinSender _underlyingSender;
        private double _bucket;
        private readonly Stopwatch _timeSinceLastRequest;

        private int _requestsReceived;
        private int _throttledRequests;
        private readonly TimeSpan _logPeriod;
        private readonly Stopwatch _currentLogPeriod;

        public int MaxSendRequests { get; private set; }
        public TimeSpan Duration { get; private set; }
        public double Rate { get; private set; }

        /// <summary>
        /// Instantiate a rate limited IZipkinSender which use an underlying IZipkinSender to send data.
        /// The average sending rate is given by maxSendRequest / duration, instantaneous rate can be much higher.
        /// </summary>
        /// <param name="sender">The underlying IZipkinSender</param>
        /// <param name="maxSendRequest">The maximum send requests per duration aka the bucket size</param>
        /// <param name="duration">TimeSpan necessary to refill the bucket</param>
        public RateLimiterZipkinSender(IZipkinSender sender, int maxSendRequest = 15, TimeSpan? duration = null)
        {
            _underlyingSender = sender;
            MaxSendRequests = maxSendRequest;
            Duration = duration ?? TimeSpan.FromSeconds(1);

            _logPeriod = TimeSpan.FromTicks(Duration.Ticks * 10);
            _requestsReceived = 0;
            _throttledRequests = 0;
            _currentLogPeriod = new Stopwatch();

            _bucket = MaxSendRequests;
            _timeSinceLastRequest = Stopwatch.StartNew();
            Rate = MaxSendRequests / Duration.TotalMilliseconds;
        }

        /// <summary>
        /// Send the data if the current instance is not throttled, discard the data otherwise.
        /// </summary>
        /// <param name="data"></param>
        public void Send(byte[] data)
        {
            lock (_lockObject)
            {
                _requestsReceived++;
                if (DecrementBucket())
                    _underlyingSender.Send(data);
                else
                {
                    _throttledRequests++;
                    StartLogTimer();
                }
                if (!ShouldLogThrottling())
                    return;
                LogThrottling();
                ResetLogCountersAndTimer();
            }
        }

        private bool DecrementBucket()
        {
            FillBucket(_timeSinceLastRequest.ElapsedMilliseconds);
            _timeSinceLastRequest.Restart();
            if (_bucket > MaxSendRequests)
                _bucket = MaxSendRequests;
            if (_bucket < 1)
                return false;
            _bucket--;
            return true;
        }

        private void FillBucket(long elapsedMsSinceLastRequest)
        {
            _bucket += elapsedMsSinceLastRequest * Rate;
        }

        private void StartLogTimer()
        {
            if (_currentLogPeriod.IsRunning)
                return;
            _currentLogPeriod.Start();
            _requestsReceived = 1;
        }

        private bool ShouldLogThrottling()
        {
            return _currentLogPeriod.IsRunning && _currentLogPeriod.ElapsedMilliseconds >= _logPeriod.TotalMilliseconds;
        }

        private void LogThrottling()
        {
            var logMsg = string.Format("{0}/{1} traces throttled in {2} ms", _throttledRequests, _requestsReceived, _currentLogPeriod.ElapsedMilliseconds);
            TraceManager.Logger.LogWarning(logMsg);
        }

        private void ResetLogCountersAndTimer()
        {
            _requestsReceived = 0;
            _throttledRequests = 0;
            _currentLogPeriod.Reset();
        }
    }
}
