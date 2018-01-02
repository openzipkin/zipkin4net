using System;

namespace zipkin4net
{
    /// <summary>
    /// Simple interface users can customize a span with. For example, this can add custom tags useful
    /// in looking up spans.
    /// 
    /// <p>This type is safer to expose directly to users than {@link Span}, as it has no hooks that
    /// can affect the span lifecycle.</p>
    /// </summary>
    //TODO internal for now. It's still work in progress
    internal interface ISpanCustomizer
    {
        /// <summary>
        /// Sets the string name for the logical operation this span represents.
        /// </summary>
        ISpan Name(string name);
        
        /// <summary>
        /// Associates an event that explains latency with the current system time.
        /// </summary>
        /// <param name="value">A short tag indicating the event, like "finagle.retry"</param>
        ISpan Annotate(string value);
        
        /// <summary>
        /// Like <see cref="Annotate(string)"/>, except with a given timestamp in microseconds.
        /// </summary>
        ISpan Annotate(DateTime timestamp, string value);

        /// <summary>
        /// Tags give your span context for search, viewing and analysis. For example, a key
        /// "your_app.version" would let you lookup spans by version.
        /// </summary>
        /// <param name="key">Name used to lookup spans, such as "your_app.version".</param>
        /// <param name="value">String value, cannot be <code>null</code></param>
        ISpan Tag(string key, string value);
    }
}