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

        /// <summary>
        /// Cast binary object Value to one of the following types :
        /// string, bool, short, int, long, byte[], double
        /// </summary>
        /// <param name="binaryAnnotation"></param>
        public void Visit(Annotation.BinaryAnnotation binaryAnnotation)
        {
            if (binaryAnnotation.Value is string)
            {
                var bytes = BinaryAnnotationValueEncoder.Encode((string)binaryAnnotation.Value);
                _span.AddBinaryAnnotation(new BinaryAnnotation(binaryAnnotation.Key, bytes, AnnotationType.STRING));
            }
            else if (binaryAnnotation.Value is bool)
            {
                var bytes = BinaryAnnotationValueEncoder.Encode((bool)binaryAnnotation.Value);
                _span.AddBinaryAnnotation(new BinaryAnnotation(binaryAnnotation.Key, bytes, AnnotationType.BOOL));
            }
            else if (binaryAnnotation.Value is short)
            {
                var bytes = BinaryAnnotationValueEncoder.Encode((short)binaryAnnotation.Value);
                _span.AddBinaryAnnotation(new BinaryAnnotation(binaryAnnotation.Key, bytes, AnnotationType.I16));
            }
            else if (binaryAnnotation.Value is int)
            {
                var bytes = BinaryAnnotationValueEncoder.Encode((int)binaryAnnotation.Value);
                _span.AddBinaryAnnotation(new BinaryAnnotation(binaryAnnotation.Key, bytes, AnnotationType.I32));
            }
            else if (binaryAnnotation.Value is long)
            {
                var bytes = BinaryAnnotationValueEncoder.Encode((long)binaryAnnotation.Value);
                _span.AddBinaryAnnotation(new BinaryAnnotation(binaryAnnotation.Key, bytes, AnnotationType.I64));
            }
            else if (binaryAnnotation.Value is byte[])
            {
                var bytes = (byte[])(binaryAnnotation.Value);
                _span.AddBinaryAnnotation(new BinaryAnnotation(binaryAnnotation.Key, bytes, AnnotationType.BYTES));
            }
            else if (binaryAnnotation.Value is double)
            {
                var bytes = BinaryAnnotationValueEncoder.Encode((double)binaryAnnotation.Value);
                _span.AddBinaryAnnotation(new BinaryAnnotation(binaryAnnotation.Key, bytes, AnnotationType.DOUBLE));
            }
            else
            {
                throw new ArgumentException("Unsupported object type for binary annotation.");
            }

        }

    }
}
