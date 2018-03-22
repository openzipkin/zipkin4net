using System;
using zipkin4net.Tracers.Zipkin;
using zipkin4net.Utils;

namespace zipkin4net.Internal.Recorder
{
    internal class Recorder : IRecorder
    {
        private readonly Endpoint _localEndPoint;
        private readonly MutableSpanMap _spanMap;

        internal Recorder(Endpoint localEndPoint, IReporter<V2.Span> reporter)
        {
            _localEndPoint = localEndPoint;
            _spanMap = new MutableSpanMap(reporter, new Statistics()); //todo inject statistics
        }

        public void Start(ITraceContext context)
        {
            var span = _spanMap.GetOrCreate(context, (t) => new MutableSpan(t, _localEndPoint));
            span.Start(TimeUtils.UtcNow);
        }

        public void Name(ITraceContext context, string name)
        {
            var span = GetSpan(context);
            span.Name(name);
        }

        private MutableSpan GetSpan(ITraceContext context)
        {
            return _spanMap.Get(context);
        }

        public void Kind(ITraceContext context, SpanKind kind)
        {
            var span = GetSpan(context);
            span.Kind(kind);
        }

        public void RemoteEndPoint(ITraceContext context, Endpoint remoteEndPoint)
        {
            var span = GetSpan(context);
            span.RemoteEndPoint(remoteEndPoint);
        }

        public void Annotate(ITraceContext context, string value)
        {
            Annotate(context, TimeUtils.UtcNow, value);
        }

        public void Annotate(ITraceContext context, DateTime timestamp, string value)
        {
            var span = GetSpan(context);
            span.Annotate(timestamp, value);
        }

        public void Tag(ITraceContext context, string key, string value)
        {
            var span = GetSpan(context);
            span.Tag(key, value);
        }

        public void Finish(ITraceContext context)
        {
            Finish(context, TimeUtils.UtcNow);
        }

        public void Finish(ITraceContext context, DateTime finishTimestamp)
        {
            var span = GetSpan(context);
            span.Finish(finishTimestamp);
            _spanMap.RemoveThenReportSpan(context);
        }

        public void Abandon(ITraceContext context)
        {
            _spanMap.Remove(context);
        }

        public void Flush(ITraceContext context)
        {
            var span = GetSpan(context);
            if (span == null)
            {
                return;
            }
            span.Annotate(TimeUtils.UtcNow, "flush.timeout");
            span.Finish(default(DateTime));
            _spanMap.RemoveThenReportSpan(context);
        }
    }
}