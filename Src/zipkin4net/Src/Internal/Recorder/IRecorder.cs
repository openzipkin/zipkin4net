using System;

namespace zipkin4net.Internal.Recorder
{
    //TODO internal for now. It's still work in progress
    internal interface IRecorder
    {
        void Start(ITraceContext context);
        void Start(ITraceContext context, DateTime timestamp);
        void Name(ITraceContext context, string name);
        void Kind(ITraceContext context, SpanKind kind);
        void RemoteEndPoint(ITraceContext context, IEndPoint remoteEndPoint);
        void Annotate(ITraceContext context, string value);
        void Annotate(ITraceContext context, DateTime timestamp, string value);
        void Tag(ITraceContext context, string key, string value);
        void Tag(ITraceContext context, DateTime timstamp, string key, string value);
        void Finish(ITraceContext context);
        void Finish(ITraceContext context, DateTime finishTimestamp);
        void Abandon(ITraceContext context);
        void Flush(ITraceContext context);
    }
}