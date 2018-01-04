namespace zipkin4net
{
    public interface ISamplingFlags
    {
        /// <summary>
        /// Should we sample this request or not? True means sample, false means don't, null means we defer
        /// decision to someone further down in the stack.
        /// </summary>
        bool? Sampled { get; }

        /// <summary>
        /// True is a request to store this span even if it overrides sampling policy. Defaults to false.
        /// </summary>
        bool Debug { get; }
    }
}