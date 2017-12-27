namespace zipkin4net.Propagation
{
    /// <summary>
    /// Used to send the trace context downstream. For example, as http headers.
    /// </summary>
    public interface IInjector<C>
    {
        /// <summary>
        /// Usually calls a setter for each propagation field to send downstream.
        /// </summary>
        /// <returns>The inject.</returns>
        /// <param name="traceContext">possibly unsampled.</param>
        /// <param name="carrier">holds propagation fields. For example, an outgoing message or http request.</param>
        void Inject(ITraceContext traceContext, C carrier);
    }
}
