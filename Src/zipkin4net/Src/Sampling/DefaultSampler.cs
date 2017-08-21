using System;
using System.Threading;

namespace zipkin4net.Sampling
{
    internal class DefaultSampler : ISampler
    {
        // Avoid that every machines sample the same traceId subset
        private readonly long _salt;

        private float _samplingRate;
        private const int RatePrecision = 1000000;

        public DefaultSampler(long salt, float samplingRate = 0f)
        {
            _salt = salt;
            SamplingRate = samplingRate;
        }

        public float SamplingRate
        {
            get { return _samplingRate; }
            set
            {
                if (!IsValidSamplingRate(value))
                    throw new ArgumentOutOfRangeException("value", "Sample rate should be between 0.0 and 1.0");

                Interlocked.Exchange(ref _samplingRate, value);
            }
        }

        public bool Sample(long traceId)
        {
            return Math.Abs(traceId ^ _salt) % RatePrecision < (_samplingRate * RatePrecision);
        }

        private static bool IsValidSamplingRate(float rate)
        {
            return 0.0f <= rate && rate <= 1.0f;
        }
    }
}
