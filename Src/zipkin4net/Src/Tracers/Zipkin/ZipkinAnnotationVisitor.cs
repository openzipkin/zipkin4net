using System;
using System.Collections.Generic;
using System.Net;
using zipkin4net.Annotation;
using zipkin4net.Tracers.Zipkin.Thrift;

namespace zipkin4net.Tracers.Zipkin
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

        public void Visit(ProducerStart producerStart)
        {
            AddTimestampedAnnotation(zipkinCoreConstants.MESSAGE_SEND);
        }

        public void Visit(ProducerStop producerStop)
        {
            _span.SetAsComplete(_record.Timestamp);
        }

        public void Visit(ConsumerStart consumerStart)
        {
            AddTimestampedAnnotation(zipkinCoreConstants.MESSAGE_RECV);
        }

        public void Visit(ConsumerStop consumerStop)
        {
            _span.SetAsComplete(_record.Timestamp);
        }

        public void Visit(MessageAddr messageAddr)
        {
            AddBinaryAnnotation(zipkinCoreConstants.MESSAGE_ADDR, true, messageAddr.ServiceName, messageAddr.Endpoint);
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

        public void Visit(ClientAddr clientAddr)
        {
            string serviceName = null;
            AddBinaryAnnotation(zipkinCoreConstants.CLIENT_ADDR, true, serviceName, clientAddr.Endpoint);
        }

        public void Visit(ServerAddr serverAddr)
        {
            AddBinaryAnnotation(zipkinCoreConstants.SERVER_ADDR, true, serverAddr.ServiceName, serverAddr.Endpoint);
        }


        /// <summary>
        /// Cast binary object Value to one of the following types :
        /// string, bool, short, int, long, byte[], double
        /// </summary>
        private void AddBinaryAnnotation(string annotationKey, object annotationValue, string serviceName = null, IPEndPoint endpoint = null)
        {
            var annotationType = GetAnnotationType(annotationValue);
            var bytes = EncodeValue(annotationValue, annotationType);
            _span.AddBinaryAnnotation(new BinaryAnnotation(annotationKey, bytes, annotationType, _record.Timestamp, serviceName, endpoint));
        }

        private static Dictionary<Type, AnnotationType> thriftTypes = new Dictionary<Type, AnnotationType>()
        {
            { typeof(string), AnnotationType.STRING },
            { typeof(bool), AnnotationType.BOOL },
            { typeof(short), AnnotationType.I16 },
            { typeof(int), AnnotationType.I32 },
            { typeof(long), AnnotationType.I64 },
            { typeof(byte[]), AnnotationType.BYTES },
            { typeof(double), AnnotationType.DOUBLE }
        };

        private static byte[] EncodeValue(object annotationValue, AnnotationType annotationType)
        {
            switch (annotationType)
            {
                case AnnotationType.STRING:
                    return BinaryAnnotationValueEncoder.Encode((string)annotationValue);
                case AnnotationType.BOOL:
                    return BinaryAnnotationValueEncoder.Encode((bool)annotationValue);
                case AnnotationType.I16:
                    return BinaryAnnotationValueEncoder.Encode((short)annotationValue);
                case AnnotationType.I32:
                    return BinaryAnnotationValueEncoder.Encode((int)annotationValue);
                case AnnotationType.I64:
                    return BinaryAnnotationValueEncoder.Encode((long)annotationValue);
                case AnnotationType.BYTES:
                    return (byte[])(annotationValue);
                case AnnotationType.DOUBLE:
                    return BinaryAnnotationValueEncoder.Encode((double)annotationValue);
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
            if (thriftTypes.TryGetValue(type, out thriftType))
            {
                return thriftType;
            }
            throw new ArgumentException("Unsupported object type for binary annotation.");
        }
    }
}