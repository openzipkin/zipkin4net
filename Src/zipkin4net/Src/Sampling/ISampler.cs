namespace Criteo.Profiling.Tracing.Sampling
{
    internal interface ISampler
    {

        float SamplingRate { get; set; }

        bool Sample(long traceId);

    }
}
