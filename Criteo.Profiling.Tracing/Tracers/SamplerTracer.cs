using Criteo.Profiling.Tracing.Sampling;
using Criteo.Profiling.Tracing.Utils;

namespace Criteo.Profiling.Tracing.Tracers
{

    /// <summary>
    /// C# version of Finagle sampler tracer (com.twitter.finagle.zipkin.thrift.Sampler)
    /// </summary>
    public class SamplerTracer : ITracer
    {

        private readonly ISampler _sampler;
        private readonly ITracer _underlyingTracer;

        public float SamplingRate
        {
            get { return _sampler.SamplingRate; }
            set { _sampler.SamplingRate = value; }
        }

        public SamplerTracer(ITracer underlyingTracer)
            : this(underlyingTracer, new DefaultSampler(RandomUtils.NextLong()))
        {

        }

        internal SamplerTracer(ITracer underlyingTracer, ISampler sampler)
        {
            _underlyingTracer = underlyingTracer;
            _sampler = sampler;
        }

        public void Record(Record record)
        {
            if (Sample(record.SpanId))
            {
                _underlyingTracer.Record(record);
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
                return _sampler.Sample(spanId.TraceId);
            }
        }

    }
}
