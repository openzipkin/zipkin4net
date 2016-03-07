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
            if (Sample(record.SpanState))
            {
                _underlyingTracer.Record(record);
            }
        }

        /// <summary>
        /// Determines if the trace should be sampled or not.
        /// If the sampling is known it uses the "sampled" value.
        /// </summary>
        /// <param name="spanState"></param>
        /// <returns></returns>
        private bool Sample(SpanState spanState)
        {
            if (spanState.Flags.HasFlag(SpanFlags.SamplingKnown))
            {
                return spanState.Flags.HasFlag(SpanFlags.Sampled);
            }
            //Backward compatibility mode. If sample flag is not set,
            //the fact that the trace exists means that it is sampled
            return true;
        }

    }
}
