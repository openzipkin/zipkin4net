using System;
using Criteo.Profiling.Tracing.Annotation;
using Criteo.Profiling.Tracing.Tracers.Zipkin.Thrift;

namespace Criteo.Profiling.Tracing.Tracers.Zipkin
{
    internal class ZipkinAnnotationVisitor : IAnnotationVisitor
    {
        private readonly Span _span;
        private readonly Record _record;

        public ZipkinAnnotationVisitor(Record record, Span span)
        {
            _span = span;
            _record = record;
        }

        public void Visit(ClientRecv clientRecv)
        {
            AddTimestampedAnnotation(zipkinCoreConstants.CLIENT_RECV);
            _span.SetAsComplete(_record.Timestamp);
        }

        public void Visit(ClientSend clientSend)
        {
            AddTimestampedAnnotation(zipkinCoreConstants.CLIENT_SEND);
        }

        public void Visit(ServerRecv serverRecv)
        {
            AddTimestampedAnnotation(zipkinCoreConstants.SERVER_RECV);
        }

        public void Visit(ServerSend serverSend)
        {
            AddTimestampedAnnotation(zipkinCoreConstants.SERVER_SEND);
            _span.SetAsComplete(_record.Timestamp);
        }

        public void Visit(WireSend wireSend)
        {
            AddTimestampedAnnotation(zipkinCoreConstants.WIRE_SEND);
        }

        public void Visit(WireRecv wireRecv)
        {
            AddTimestampedAnnotation(zipkinCoreConstants.WIRE_RECV);
        }

        public void Visit(Event ev)
        {
            AddTimestampedAnnotation(ev.EventName);
        }

        private void AddTimestampedAnnotation(string value)
        {
            _span.AddAnnotation(new ZipkinAnnotation(_record.Timestamp, value));
        }

        public void Visit(Rpc rpc)
        {
            _span.Name = rpc.Name;
        }

        public void Visit(ServiceName serviceName)
        {
            _span.ServiceName = serviceName.Service;
        }

        public void Visit(LocalAddr localAddr)
        {
            _span.Endpoint = localAddr.EndPoint;
        }

        public void Visit(LocalOperationStop operation)
        {
            _span.SetAsComplete(_record.Timestamp);
        }

        public void Visit(LocalOperationStart localOperation)
        {
            AddBinaryAnnotation(zipkinCoreConstants.LOCAL_COMPONENT, localOperation.OperationName);
        }

        public void Visit(TagAnnotation tagAnnotation)
        {
            AddBinaryAnnotation(tagAnnotation.Key, tagAnnotation.Value);
        }

        public void Visit(ClientAddress clientAddress)
        {
            AddBinaryAnnotation(zipkinCoreConstants.CLIENT_ADDR, clientAddress.Address);
        }

        public void Visit(ServerAddress serverAddress)
        {
            AddBinaryAnnotation(zipkinCoreConstants.SERVER_ADDR, serverAddress.Address);
        }

        /// <summary>
        /// Cast binary object Value to one of the following types :
        /// string, bool, short, int, long, byte[], double
        /// </summary>
        /// <param name="annotationKey"></param>
        /// <param name="annotationValue"></param>
        private void AddBinaryAnnotation(string annotationKey, object annotationValue)
        {
            if (annotationValue is string)
            {
                var bytes = BinaryAnnotationValueEncoder.Encode((string)annotationValue);
                _span.AddBinaryAnnotation(new BinaryAnnotation(annotationKey, bytes, AnnotationType.STRING, _record.Timestamp));
            }
            else if (annotationValue is bool)
            {
                var bytes = BinaryAnnotationValueEncoder.Encode((bool)annotationValue);
                _span.AddBinaryAnnotation(new BinaryAnnotation(annotationKey, bytes, AnnotationType.BOOL, _record.Timestamp));
            }
            else if (annotationValue is short)
            {
                var bytes = BinaryAnnotationValueEncoder.Encode((short)annotationValue);
                _span.AddBinaryAnnotation(new BinaryAnnotation(annotationKey, bytes, AnnotationType.I16, _record.Timestamp));
            }
            else if (annotationValue is int)
            {
                var bytes = BinaryAnnotationValueEncoder.Encode((int)annotationValue);
                _span.AddBinaryAnnotation(new BinaryAnnotation(annotationKey, bytes, AnnotationType.I32, _record.Timestamp));
            }
            else if (annotationValue is long)
            {
                var bytes = BinaryAnnotationValueEncoder.Encode((long)annotationValue);
                _span.AddBinaryAnnotation(new BinaryAnnotation(annotationKey, bytes, AnnotationType.I64, _record.Timestamp));
            }
            else if (annotationValue is byte[])
            {
                var bytes = (byte[])(annotationValue);
                _span.AddBinaryAnnotation(new BinaryAnnotation(annotationKey, bytes, AnnotationType.BYTES, _record.Timestamp));
            }
            else if (annotationValue is double)
            {
                var bytes = BinaryAnnotationValueEncoder.Encode((double)annotationValue);
                _span.AddBinaryAnnotation(new BinaryAnnotation(annotationKey, bytes, AnnotationType.DOUBLE, _record.Timestamp));
            }
            else
            {
                throw new ArgumentException("Unsupported object type for binary annotation.");
            }
        }
    }
}