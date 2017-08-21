namespace zipkin4net.Sampling
{
    internal interface ISampler
    {

        float SamplingRate { get; set; }

        bool Sample(long traceId);

    }
}
