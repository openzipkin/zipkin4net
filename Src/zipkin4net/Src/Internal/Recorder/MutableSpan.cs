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

        internal MutableSpan LocalEndpoint(V2.Endpoint endpoint)
        {
            _span.LocalEndpoint(endpoint);
            return this;
        }

        internal MutableSpan RemoteEndPoint(V2.Endpoint endpoint)
        {
            _span.RemoteEndpoint(endpoint);
            return this;
        }

        internal MutableSpan Annotate(DateTime timestamp, string value)
        {
            switch (value)
            {
                case "cs":
                    Timestamp = timestamp;
                    _span
                        .Kind(Span.SpanKind.CLIENT)
                        .Timestamp(timestamp);
                    break;
                case "sr":
                    Timestamp = timestamp;
                    _span
                        .Kind(Span.SpanKind.SERVER)
                        .Timestamp(timestamp);
                    break;
                case "cr":
                    _span.Kind(Span.SpanKind.CLIENT);
                    Finish(timestamp);
                    break;
                case "ss":
                    _span.Kind(Span.SpanKind.SERVER);
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