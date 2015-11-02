using System;
using System.Text;
using Criteo.Profiling.Tracing.Annotation;
using Criteo.Profiling.Tracing.Tracers.Zipkin.Thrift;

namespace Criteo.Profiling.Tracing.Tracers.Zipkin
{
    internal class ZipkinAnnotationVisitor : IAnnotationVisitor
    {
        private readonly Span span;
        private readonly Record record;

        public ZipkinAnnotationVisitor(Record record, Span span)
        {
            this.span = span;
            this.record = record;
        }

        public void Visit(ClientRecv clientRecv)
        {
            span.AddAnnotation(new ZipkinAnnotation(record.Timestamp, zipkinCoreConstants.CLIENT_RECV));
        }

        public void Visit(ClientSend clientSend)
        {
            span.AddAnnotation(new ZipkinAnnotation(record.Timestamp, zipkinCoreConstants.CLIENT_SEND));
        }

        public void Visit(ServerRecv serverRecv)
        {
            span.AddAnnotation(new ZipkinAnnotation(record.Timestamp, zipkinCoreConstants.SERVER_RECV));
        }

        public void Visit(ServerSend serverSend)
        {
            span.AddAnnotation(new ZipkinAnnotation(record.Timestamp, zipkinCoreConstants.SERVER_SEND));
        }

        public void Visit(Rpc rpc)
        {
            span.Name = rpc.Name;
        }

        public void Visit(ServiceName serviceName)
        {
            span.ServiceName = serviceName.Service;
        }

        public void Visit(LocalAddr localAddr)
        {
            span.Endpoint = localAddr.EndPoint;
        }

        /// <summary>
        /// Cast binary object Value to one of the following types :
        /// string, bool, int16, int32, int64, byte[], double
        /// </summary>
        /// <param name="binaryAnnotation"></param>
        public void Visit(Annotation.BinaryAnnotation binaryAnnotation)
        {
            if (binaryAnnotation.Value is string)
            {
                var strValue = (String)(binaryAnnotation.Value);
                AddBinaryString(binaryAnnotation, strValue);
            }
            else if (binaryAnnotation.Value is bool)
            {
                var bValue = (bool)(binaryAnnotation.Value);
                AddBinaryBool(binaryAnnotation, bValue);
            }
            else if (binaryAnnotation.Value is Int16)
            {
                var iValue = (Int16)(binaryAnnotation.Value);
                AddBinaryInt16(binaryAnnotation, iValue);
            }
            else if (binaryAnnotation.Value is Int32)
            {
                var iValue = (Int32)(binaryAnnotation.Value);
                AddBinaryInt32(binaryAnnotation, iValue);
            }
            else if (binaryAnnotation.Value is Int64)
            {
                var iValue = (Int64)(binaryAnnotation.Value);
                AddBinaryInt64(binaryAnnotation, iValue);
            }
            else if (binaryAnnotation.Value is byte[])
            {
                var bytes = (byte[])(binaryAnnotation.Value);
                AddBinaryBytes(binaryAnnotation, bytes);
            }
            else if (binaryAnnotation.Value is double)
            {
                var dValue = (double)(binaryAnnotation.Value);
                AddBinaryDouble(binaryAnnotation, dValue);
            }
            else
            {
                throw new ArgumentException("Unsupported object type for binary annotation.");
            }
        }

        private void AddBinaryString(Annotation.BinaryAnnotation binaryAnnotation, string value)
        {
            var bytes = Encoding.ASCII.GetBytes(value);
            AddBinaryAnnotation(binaryAnnotation.Key, bytes, AnnotationType.STRING);
        }

        private void AddBinaryBool(Annotation.BinaryAnnotation binaryAnnotation, bool value)
        {
            var bytes = BitConverter.GetBytes(value);
            AddBinaryAnnotation(binaryAnnotation.Key, bytes, AnnotationType.BOOL);
        }

        private void AddBinaryInt16(Annotation.BinaryAnnotation binaryAnnotation, Int16 value)
        {
            var bytes = BitConverter.GetBytes(value);
            AddBinaryAnnotation(binaryAnnotation.Key, bytes, AnnotationType.I16);
        }

        private void AddBinaryInt32(Annotation.BinaryAnnotation binaryAnnotation, Int32 value)
        {
            var bytes = BitConverter.GetBytes(value);
            AddBinaryAnnotation(binaryAnnotation.Key, bytes, AnnotationType.I32);
        }

        private void AddBinaryInt64(Annotation.BinaryAnnotation binaryAnnotation, Int64 value)
        {
            var bytes = BitConverter.GetBytes(value);
            AddBinaryAnnotation(binaryAnnotation.Key, bytes, AnnotationType.I64);
        }

        private void AddBinaryBytes(Annotation.BinaryAnnotation binaryAnnotation, byte[] bytes)
        {
            var annotation = new BinaryAnnotation(binaryAnnotation.Key, bytes, AnnotationType.BYTES);
            span.AddBinaryAnnotation(annotation);
        }

        private void AddBinaryDouble(Annotation.BinaryAnnotation binaryAnnotation, double value)
        {
            var bytes = BitConverter.GetBytes(value);
            AddBinaryAnnotation(binaryAnnotation.Key, bytes, AnnotationType.DOUBLE);
        }

        private void AddBinaryAnnotation(String key, byte[] value, AnnotationType type)
        {
            span.AddBinaryAnnotation(new BinaryAnnotation(key, value, type));
        }

    }
}
