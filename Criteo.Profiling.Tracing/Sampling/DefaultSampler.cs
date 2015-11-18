using System;
using System.Threading;
using Criteo.Profiling.Tracing.Utils;

namespace Criteo.Profiling.Tracing.Sampling
{
    internal class DefaultSampler : ISampler
    {
        // Avoid that every machines sample the same traceId subset
        private static readonly long Salt = RandomUtils.NextLong();

        private float _samplingRate;

        public DefaultSampler(float samplingRate = 0f)
        {
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
            return Math.Abs(traceId ^ Salt) % 10000 < (_samplingRate * 10000);
        }

        private static bool IsValidSamplingRate(float rate)
        {
            return 0.0f <= rate && rate <= 1.0f;
        }
    }
}
