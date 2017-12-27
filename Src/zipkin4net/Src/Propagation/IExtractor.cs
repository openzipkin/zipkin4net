namespace zipkin4net.Propagation
{
    /// <summary>
    /// Used to join an incoming trace. For example, by reading http headers.
    /// </summary>
    public interface IExtractor<C> {
        
        /// <summary>
        /// Returns a trace context parsed from the carrier.
        /// </summary>
        /// <returns>The extract.</returns>
        /// <param name="carrier">holds propagation fields. For example, an incoming message or http request.</param>
        ITraceContext Extract(C carrier);
    }
}
