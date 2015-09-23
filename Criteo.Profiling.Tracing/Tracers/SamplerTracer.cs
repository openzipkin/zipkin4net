using System;
using System.Threading;
using Criteo.Profiling.Tracing.Utils;

namespace Criteo.Profiling.Tracing.Tracers
{

    /// <summary>
    /// C# version of Finagle sampler tracer (com.twitter.finagle.zipkin.thrift.Sampler)
    /// </summary>
    public class SamplerTracer : ITracer
    {
        // Avoid that every machines sample the same traceId subset
        private static readonly long salt = RandomUtils.NextLong();

        private float sampleRate;
        private readonly ITracer underlyingTracer;

        public float SampleRate
        {
            get { return sampleRate; }

            set
            {
                if (!IsValidSamplingRate(value))
                    throw new ArgumentOutOfRangeException("value", "Sample rate should be between 0.0 and 1.0");

                Interlocked.Exchange(ref sampleRate, value);
            }
        }

        public SamplerTracer(ITracer underlyingTracer, float sampleRate = 0.001f)
        {
            this.underlyingTracer = underlyingTracer;
            this.SampleRate = sampleRate;
        }

        public void Record(Record record)
        {
            if (Sample(record.SpanId))
            {
                underlyingTracer.Record(record);
            }
        }

        /// <summary>
        /// Determines if the trace should be sampled or not.
        /// If the sampling is known it uses the "sampled" value. Otherwise randomly decides.
        /// </summary>
        /// <param name="spanId"></param>
        /// <returns></returns>
        private bool Sample(SpanId spanId)
        {
            if (spanId.Flags.IsSamplingKnown())
            {
                return spanId.Flags.IsSampled();
            }
            else
            {
                return RandomSample(spanId.TraceId);
            }
        }

        private bool RandomSample(long traceId)
        {
            return Math.Abs(traceId ^ salt) % 10000 < (SampleRate * 10000);
        }

        private static bool IsValidSamplingRate(float rate)
        {
            return 0.0f <= rate && rate <= 1.0f;
        }

    }
}
