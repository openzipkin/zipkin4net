namespace Criteo.Profiling.Tracing.Tracers
{

    /// <summary>
    /// C# version of Finagle sampler tracer (com.twitter.finagle.zipkin.thrift.Sampler)
    /// </summary>
    public class SamplerTracer : ITracer
    {
        private readonly ITracer _underlyingTracer;

        public SamplerTracer(ITracer underlyingTracer)
        {
            _underlyingTracer = underlyingTracer;
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
        /// If the sampling is known it uses the "sampled" value.
        /// </summary>
        /// <param name="spanId"></param>
        /// <returns></returns>
        private bool Sample(SpanId spanId)
        {
            if (spanId.Flags.IsSamplingKnown())
            {
                return spanId.Flags.IsSampled();
            }
            //Backward compatibility mode. If sample flag is not set,
            //the fact that the trace exists means that it is sampled
            return true;
        }

    }
}
