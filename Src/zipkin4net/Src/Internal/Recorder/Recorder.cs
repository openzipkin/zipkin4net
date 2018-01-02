using System;
using System.Collections.Generic;
using System.Net;
using zipkin4net.Tracers.Zipkin;
using zipkin4net.Tracers.Zipkin.Thrift;
using zipkin4net.Utils;
using Span = zipkin4net.Tracers.Zipkin.Span;

namespace zipkin4net.Internal.Recorder
{
    internal class Recorder : IRecorder
    {
        private readonly IEndPoint _localEndPoint;
        private readonly MutableSpanMap _spanMap;

        internal Recorder(IEndPoint localEndPoint, IReporter reporter)
        {
            _localEndPoint = localEndPoint;
            _spanMap = new MutableSpanMap(reporter, new Statistics()); //todo inject statistics
        }

        public void Start(ITraceContext context)
        {
            Start(context, TimeUtils.UtcNow);
        }

        public void Start(ITraceContext context, DateTime timestamp)
        {
            _spanMap.GetOrCreate(context, (t) => new Span(t, timestamp));
        }

        public void Name(ITraceContext context, string name)
        {
            var span = GetSpan(context);
            span.Name = name;
        }

        [Obsolete("Backward compatibility. Exclusively for ZipkinAnnotationVisitor")]
        internal void ServiceName(ITraceContext context, string serviceName)
        {
            var span = GetSpan(context);
            span.ServiceName = serviceName;
        }

        [Obsolete("Backward compatibility. Exclusively for ZipkinAnnotationVisitor")]
        internal void EndPoint(ITraceContext context, IPEndPoint endPoint)
        {
            var span = GetSpan(context);
            span.Endpoint = endPoint;
        }

        private Span GetSpan(ITraceContext context)
        {
            return _spanMap.Get(context);
        }

        public void Kind(ITraceContext context, SpanKind kind)
        {
            var span = GetSpan(context);
            span.SpanKind = kind;
        }

        public void RemoteEndPoint(ITraceContext context, IEndPoint remoteEndPoint)
        {
            var span = GetSpan(context);
            if (span.SpanKind.HasValue)
            {
                switch (span.SpanKind.Value)
                {
                    case SpanKind.Client:
                        AddBinaryAnnotation(span, TimeUtils.UtcNow, zipkinCoreConstants.SERVER_ADDR, true,
                            remoteEndPoint);
                        break;
                    case SpanKind.Server:
                        AddBinaryAnnotation(span, TimeUtils.UtcNow, zipkinCoreConstants.CLIENT_ADDR, true,
                            remoteEndPoint);
                        break;
                    case SpanKind.Producer:
                        AddBinaryAnnotation(span, TimeUtils.UtcNow, zipkinCoreConstants.MESSAGE_ADDR, true,
                            remoteEndPoint);
                        break;
                    case SpanKind.Consumer:
                        AddBinaryAnnotation(span, TimeUtils.UtcNow, zipkinCoreConstants.MESSAGE_ADDR, true,
                            remoteEndPoint);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            AddBinaryAnnotation(span, TimeUtils.UtcNow, zipkinCoreConstants.SERVER_ADDR, true,
                remoteEndPoint);
        }

        public void Annotate(ITraceContext context, string value)
        {
            Annotate(context, TimeUtils.UtcNow, value);
        }

        public void Annotate(ITraceContext context, DateTime timestamp, string value)
        {
            var span = GetSpan(context);
            span.AddAnnotation(new ZipkinAnnotation(timestamp, value));
        }

        public void Tag(ITraceContext context, string key, string value)
        {
            Tag(context, TimeUtils.UtcNow, key, value);
        }


        public void Tag(ITraceContext context, DateTime timestamp, string key, string value)
        {
            Tag(context, timestamp, key, (object) value);
        }

        [Obsolete("Bakcward compatibility only")]
        internal void Tag(ITraceContext context, DateTime timestamp, string key, object value)
        {
            AddBinaryAnnotation(context, timestamp, key, value);
        }

        public void Finish(ITraceContext context)
        {
            Finish(context, TimeUtils.UtcNow);
        }

        public void Finish(ITraceContext context, DateTime finishTimestamp)
        {
            var span = GetSpan(context);
            if (string.IsNullOrEmpty(span.ServiceName))
            {
                span.ServiceName = _localEndPoint.ServiceName;
            }

            var kind = span.SpanKind;
            
            if (kind.HasValue)
            {
                switch (kind.Value)
                {
                    case SpanKind.Client:
                        Annotate(context, span.SpanCreated, zipkinCoreConstants.CLIENT_SEND);
                        Annotate(context, finishTimestamp, zipkinCoreConstants.CLIENT_RECV);
                        break;
                    case SpanKind.Server:
                        Annotate(context, span.SpanCreated, zipkinCoreConstants.SERVER_RECV);
                        Annotate(context, finishTimestamp, zipkinCoreConstants.SERVER_SEND);
                        break;
                    case SpanKind.Producer:
                        Annotate(context, span.SpanCreated, zipkinCoreConstants.MESSAGE_SEND);
                        break;
                    case SpanKind.Consumer:
                        Annotate(context, span.SpanCreated, zipkinCoreConstants.MESSAGE_RECV);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            span.SetAsComplete(finishTimestamp);
            _spanMap.RemoveThenReportSpan(context);
        }

        public void Abandon(ITraceContext context)
        {
            _spanMap.Remove(context);
        }

        public void Flush(ITraceContext context)
        {
            var span = GetSpan(context);
            span.AddAnnotation(new ZipkinAnnotation(TimeUtils.UtcNow, "flush.timeout"));
            span.SetAsComplete(TimeUtils.UtcNow);
            _spanMap.RemoveThenReportSpan(context);
        }


        private void AddBinaryAnnotation(ITraceContext context, DateTime timestamp, string annotationKey,
            object annotationValue)
        {
            AddBinaryAnnotation(GetSpan(context), timestamp, annotationKey, annotationValue);
        }

        private void AddBinaryAnnotation(Span span, DateTime timestamp, string annotationKey, object annotationValue)
        {
            AddBinaryAnnotation(span, timestamp, annotationKey, annotationValue, _localEndPoint);
        }

        private static void AddBinaryAnnotation(Span span, DateTime timestamp, string annotationKey,
            object annotationValue, IEndPoint endPoint)
        {
            var annotationType = GetAnnotationType(annotationValue);
            var bytes = EncodeValue(annotationValue, annotationType);
            span.AddBinaryAnnotation(new Tracers.Zipkin.BinaryAnnotation(annotationKey, bytes, annotationType,
                timestamp, endPoint.ServiceName, endPoint.IpEndPoint));
        }

        private static readonly IDictionary<Type, AnnotationType> ThriftTypes = new Dictionary<Type, AnnotationType>()
        {
            {typeof(string), AnnotationType.STRING},
            {typeof(bool), AnnotationType.BOOL},
            {typeof(short), AnnotationType.I16},
            {typeof(int), AnnotationType.I32},
            {typeof(long), AnnotationType.I64},
            {typeof(byte[]), AnnotationType.BYTES},
            {typeof(double), AnnotationType.DOUBLE}
        };


        private static byte[] EncodeValue(object annotationValue, AnnotationType annotationType)
        {
            switch (annotationType)
            {
                case AnnotationType.STRING:
                    return BinaryAnnotationValueEncoder.Encode((string) annotationValue);
                case AnnotationType.BOOL:
                    return BinaryAnnotationValueEncoder.Encode((bool) annotationValue);
                case AnnotationType.I16:
                    return BinaryAnnotationValueEncoder.Encode((short) annotationValue);
                case AnnotationType.I32:
                    return BinaryAnnotationValueEncoder.Encode((int) annotationValue);
                case AnnotationType.I64:
                    return BinaryAnnotationValueEncoder.Encode((long) annotationValue);
                case AnnotationType.BYTES:
                    return (byte[]) (annotationValue);
                case AnnotationType.DOUBLE:
                    return BinaryAnnotationValueEncoder.Encode((double) annotationValue);
            }

            throw new ArgumentException("Unsupported object type for binary annotation.");
        }

        private static AnnotationType GetAnnotationType(object annotationValue)
        {
            if (annotationValue == null)
            {
                throw new NullReferenceException("Binary annotation value can't be null");
            }

            var type = annotationValue.GetType();
            AnnotationType thriftType;
            if (ThriftTypes.TryGetValue(type, out thriftType))
            {
                return thriftType;
            }

            throw new ArgumentException("Unsupported object type for binary annotation.");
        }
    }
}