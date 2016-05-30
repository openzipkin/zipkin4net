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

        /// <summary>
        /// Cast binary object Value to one of the following types :
        /// string, bool, short, int, long, byte[], double
        /// </summary>
        /// <param name="tagAnnotation"></param>
        public void Visit(TagAnnotation tagAnnotation)
        {
            if (tagAnnotation.Value is string)
            {
                var bytes = BinaryAnnotationValueEncoder.Encode((string)tagAnnotation.Value);
                _span.AddBinaryAnnotation(new BinaryAnnotation(tagAnnotation.Key, bytes, AnnotationType.STRING));
            }
            else if (tagAnnotation.Value is bool)
            {
                var bytes = BinaryAnnotationValueEncoder.Encode((bool)tagAnnotation.Value);
                _span.AddBinaryAnnotation(new BinaryAnnotation(tagAnnotation.Key, bytes, AnnotationType.BOOL));
            }
            else if (tagAnnotation.Value is short)
            {
                var bytes = BinaryAnnotationValueEncoder.Encode((short)tagAnnotation.Value);
                _span.AddBinaryAnnotation(new BinaryAnnotation(tagAnnotation.Key, bytes, AnnotationType.I16));
            }
            else if (tagAnnotation.Value is int)
            {
                var bytes = BinaryAnnotationValueEncoder.Encode((int)tagAnnotation.Value);
                _span.AddBinaryAnnotation(new BinaryAnnotation(tagAnnotation.Key, bytes, AnnotationType.I32));
            }
            else if (tagAnnotation.Value is long)
            {
                var bytes = BinaryAnnotationValueEncoder.Encode((long)tagAnnotation.Value);
                _span.AddBinaryAnnotation(new BinaryAnnotation(tagAnnotation.Key, bytes, AnnotationType.I64));
            }
            else if (tagAnnotation.Value is byte[])
            {
                var bytes = (byte[])(tagAnnotation.Value);
                _span.AddBinaryAnnotation(new BinaryAnnotation(tagAnnotation.Key, bytes, AnnotationType.BYTES));
            }
            else if (tagAnnotation.Value is double)
            {
                var bytes = BinaryAnnotationValueEncoder.Encode((double)tagAnnotation.Value);
                _span.AddBinaryAnnotation(new BinaryAnnotation(tagAnnotation.Key, bytes, AnnotationType.DOUBLE));
            }
            else
            {
                throw new ArgumentException("Unsupported object type for binary annotation.");
            }

        }

    }
}
