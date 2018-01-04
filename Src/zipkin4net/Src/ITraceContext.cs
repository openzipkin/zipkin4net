using System.Collections.Generic;

namespace zipkin4net
{
    public interface ITraceContext : ISamplingFlags
    {
        /// <summary>
        /// When non-zero, the trace containing this span uses 128-bit trace identifiers.
        /// </summary>
        long TraceIdHigh { get; }
        /// <summary>
        /// Unique 8-byte identifier for a trace, set on all spans within it.
        /// </summary>
        long TraceId { get; }
        /// <summary>
        /// The parent's <see cref="SpanId"/> or null if this the root span in a trace.
        /// </summary>
        long? ParentSpanId { get; }
        /// <summary>
        ///  Unique 8-byte identifier of this span within a trace.
        /// 
        /// <p>A span is uniquely identified in storage by (<see cref="TraceId"/>, <see cref="SpanId"/>)</p>
        /// </summary>
        long SpanId { get; }
        /// <summary>
        /// Returns a list of additional data propagated through this trace.
        ///
        /// <p>The contents are intentionally opaque, deferring to <see cref="Propagation.IPropagation{K}"/> to define. An
        /// example implementation could be storing a class containing a correlation value, which is
        /// extracted from incoming requests and injected as-is onto outgoing requests.
        /// </summary>
        List<object> Extra { get; }
    }
}