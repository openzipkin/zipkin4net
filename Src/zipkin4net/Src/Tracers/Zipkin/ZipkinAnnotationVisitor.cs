using zipkin4net.Annotation;
using zipkin4net.Internal.Recorder;
using zipkin4net.Tracers.Zipkin.Thrift;

namespace zipkin4net.Tracers.Zipkin
{
    internal class ZipkinAnnotationVisitor : IAnnotationVisitor
    {
        private readonly MutableSpan _span;
        private readonly Record _record;

        public ZipkinAnnotationVisitor(Record record, MutableSpan span)
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

        public void Visit(ProducerStart producerStart)
        {
            AddTimestampedAnnotation(zipkinCoreConstants.MESSAGE_SEND);
        }

        public void Visit(ProducerStop producerStop)
        {
            _span.Finish(_record.Timestamp);
        }

        public void Visit(ConsumerStart consumerStart)
        {
            AddTimestampedAnnotation(zipkinCoreConstants.MESSAGE_RECV);
        }

        public void Visit(ConsumerStop consumerStop)
        {
            _span.Finish(_record.Timestamp);
        }

        public void Visit(MessageAddr messageAddr)
        {
            _span.RemoteEndPoint(new Endpoint(messageAddr.ServiceName, messageAddr.Endpoint));
        }

        public void Visit(Event ev)
        {
            AddTimestampedAnnotation(ev.EventName);
        }

        private void AddTimestampedAnnotation(string value)
        {
            _span.Annotate(_record.Timestamp, value);
        }

        public void Visit(Rpc rpc)
        {
            _span.Name(rpc.Name);
        }

        public void Visit(ServiceName serviceName)
        {
            _span.LocalEndpoint(new Endpoint(serviceName.Service, SerializerUtils.DefaultEndPoint));
        }

        public void Visit(LocalAddr localAddr)
        {
            _span.LocalEndpoint(new Endpoint(SerializerUtils.DefaultServiceName, localAddr.EndPoint));
        }

        public void Visit(LocalOperationStop operation)
        {
            _span.Finish(_record.Timestamp);
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
            _span.RemoteEndPoint(new Endpoint(serviceName, clientAddr.Endpoint));
        }

        public void Visit(ServerAddr serverAddr)
        {
            _span.RemoteEndPoint(new Endpoint(serverAddr.ServiceName, serverAddr.Endpoint));
        }

        private void AddBinaryAnnotation(string annotationKey, object annotationValue)
        {
            _span.Tag(annotationKey, annotationValue.ToString());
        }
    }
}