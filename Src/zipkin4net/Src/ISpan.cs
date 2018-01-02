using System;

namespace zipkin4net
{
    public enum SpanKind
    {
        Client,
        Server,

        /// <summary>
        /// When present, <see cref="Start"/> is the moment a consumer sent a message to a destination. A
        /// duration between <see cref="Start"/> and <see cref="Finish"/> may imply batching delay.
        /// <see cref="RemoteEndpoint(endpoint)"/> indicates the origin, such as a broker.
        ///
        /// <p>Unlike <see cref="Client"/>, messaging spans never share a span ID. For example, the
        /// <see cref="Consumer"/> of the same message has <see cref="ITraceContext.ParentSpanId"/> set to this span's
        /// <see cref="ITraceContext.SpanId"/>.</p>
        /// </summary>
        Producer,

        /// <summary>
        /// When present, <see cref="Start"/> is the moment a consumer received a message from an origin. A
        /// duration between <see cref="Start"/> and <see cref="Finish"/> may imply a processing backlog. while
        /// <see cref="RemoteEndpoint(endpoint)"/> indicates the origin, such as a broker.
        ///
        /// <p>Unlike <see cref="Server"/>, messaging spans never share a span ID. For example, the
        /// <see cref="Producer"/> of this message is the <see cref="ITraceContext.ParentSpanId"/> of this span.</p>
        /// </summary>
        Consumer
    }

    ///  <summary>
    ///  Used to model the latency of an operation.
    ///  <p>For example, to trace a local function call.</p>
    ///  <pre><code>
    ///  Span span = tracer.NewTrace().Name("encode").Start();
    ///  try {
    ///    doSomethingExpensive();
    ///  } finally {
    ///    span.Finish();
    ///  }
    ///  </code>
    ///  </pre>
    ///  This captures duration of <see cref="M:zipkin4net.ISpan.Start(System.Int64)" /> until <see cref="M:zipkin4net.ISpan.Finish(System.Int64)" /> is called.
    ///  </summary>
    //TODO internal for now. It's still work in progress
    internal interface ISpan : ISpanCustomizer
    {
        ITraceContext Context { get; }

        /// <summary>
        /// The kind of span is optional. When set, it affects how a span is reported. For example, if the
        /// kind is <see cref="SpanKind.Server"/>, the span's start timestamp is implicitly annotated as "sr" and
        /// that plus its duration as "ss".
        /// </summary>
        ISpan Kind(SpanKind kind);

        /// <summary>
        /// Starts the span with an implicit timestamp.
        /// 
        /// <p>Spans can be modified before calling start. For example, you can add tags to the span and
        /// set its name without lock contention.</p>
        /// </summary>
        /// <returns></returns>
        ISpan Start();

        /// <summary>
        /// Like <see cref="Start()"/>, except with a given timestamp in microseconds.
        /// </summary>
        /// <returns></returns>
        ISpan Start(DateTime timestamp);
        
        /// <summary>
        /// Reports the span complete, assigning the most precise duration possible.
        /// </summary>
        void Finish();

        /// <summary>
        /// Like <see cref="Finish()"/>, except with a given timestamp in microseconds.
        /// 
        /// <p>Zipkin's span duration is derived by subtracting the start
        /// timestamp from this, and set when appropriate.</p>
        /// </summary>
        /// <param name="timestamp"></param>
        void Finish(DateTime timestamp);
         
        /// <summary>
        /// Throws away the current span without reporting it.
        /// </summary>
        void Abandon();

        /// <summary>
        /// Reports the span, even if unfinished. Most users will not call this method.
        /// 
        /// <p>This primarily supports two use cases: one-way spans and orphaned spans. For example, a
        /// one-way span can be modeled as a span where one tracer calls start and another calls finish. In
        /// order to report that span from its origin, flush must be called.</p>
        ///
        /// <p>Another example is where a user didn't call finish within a deadline or before a shutdown
        /// occurs. By flushing, you can report what was in progress.</p>
        /// </summary>
        void Flush();

    }
}