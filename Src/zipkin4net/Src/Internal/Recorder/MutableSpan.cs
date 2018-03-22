using System;
using zipkin4net.Utils;
using Span = zipkin4net.Internal.V2.Span;

namespace zipkin4net.Internal.Recorder
{
    internal class MutableSpan
    {
        private readonly Span.Builder _span;
        internal DateTime Timestamp { get; private set; }
        
        internal bool Finished { get; private set; }

        public MutableSpan(ITraceContext context, Endpoint localEndpoint)
        {
            _span = new Span.Builder()
                .TraceId(TraceIdToString(context))
                .ParentId(context.ParentSpanId.HasValue && context.ParentSpanId != 0L ? NumberUtils.EncodeLongToLowerHexString(context.ParentSpanId.Value) : null)
                .Id(NumberUtils.EncodeLongToLowerHexString(context.SpanId))
                .Debug(context.Debug ? true : (bool?) null)
                .LocalEndpoint(new V2.Endpoint(localEndpoint.ServiceName, localEndpoint.IpEndPoint));
        }

        internal MutableSpan Kind(SpanKind spanKind)
        {
            _span.Kind(ToV2(spanKind));
            return this;
        }

        private static Span.SpanKind ToV2(SpanKind spanKind)
        {
            switch (spanKind)
            {
                case SpanKind.NoKind:
                    return Span.SpanKind.NoKind;
                case SpanKind.Client:
                    return Span.SpanKind.Client;
                case SpanKind.Server:
                    return Span.SpanKind.Server;
                case SpanKind.Producer:
                    return Span.SpanKind.Producer;
                case SpanKind.Consumer:
                    return Span.SpanKind.Consumer;
                default:
                    throw new ArgumentOutOfRangeException(nameof(spanKind), spanKind, null);
            }
        }

        internal MutableSpan LocalEndpoint(Endpoint endpoint)
        {
            _span.LocalEndpoint(new V2.Endpoint(endpoint.ServiceName, endpoint.IpEndPoint));
            return this;
        }

        internal MutableSpan RemoteEndPoint(Endpoint endpoint)
        {
            _span.RemoteEndpoint(new V2.Endpoint(endpoint.ServiceName, endpoint.IpEndPoint));
            return this;
        }

        internal MutableSpan Annotate(DateTime timestamp, string value)
        {
            switch (value)
            {
                case "cs":
                    Timestamp = timestamp;
                    _span
                        .Kind(Span.SpanKind.Client)
                        .Timestamp(timestamp);
                    break;
                case "sr":
                    Timestamp = timestamp;
                    _span
                        .Kind(Span.SpanKind.Server)
                        .Timestamp(timestamp);
                    break;
                case "cr":
                    _span.Kind(Span.SpanKind.Client);
                    Finish(timestamp);
                    break;
                case "ss":
                    _span.Kind(Span.SpanKind.Server);
                    Finish(timestamp);
                    break;
                default:
                    _span.AddAnnotation(timestamp, value);
                    break;
            }

            return this;
        }
        
        internal void Tag(string key, string value)
        {
            _span.PutTag(key, value);
        }
        
        internal MutableSpan Name(string name) {
            _span.Name(name);
            return this;
        }
        
        internal MutableSpan Start(DateTime timestamp)
        {
            Timestamp = timestamp;
            _span.Timestamp(timestamp);
            return this;
        }
        

        internal MutableSpan Finish(DateTime finishTimestamp)
        {
            if (Finished)
            {
                return this;
            }

            Finished = true;

            //finishTimestamp can be set to default when flushing
            if (Timestamp != default(DateTime) && finishTimestamp != default(DateTime))
            {
                _span.Duration(Math.Max((finishTimestamp.ToUnixTimestamp() - Timestamp.ToUnixTimestamp()), 1));
            }

            return this;
        }

        private static string TraceIdToString(ITraceContext traceContext)
        {
            var hexTraceId = NumberUtils.EncodeLongToLowerHexString(traceContext.TraceId);
            if (traceContext.TraceIdHigh == SpanState.NoTraceIdHigh)
            {
                return hexTraceId;
            }
            return NumberUtils.EncodeLongToLowerHexString(traceContext.TraceIdHigh) + hexTraceId;
        }

        internal Span ToSpan()
        {
            return _span.Build();
        }
    }
}