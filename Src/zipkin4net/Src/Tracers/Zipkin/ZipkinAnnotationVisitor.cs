using System;
using zipkin4net.Annotation;
using zipkin4net.Internal.Recorder;
using zipkin4net.Tracers.Zipkin.Thrift;

namespace zipkin4net.Tracers.Zipkin
{
    internal class ZipkinAnnotationVisitor : IAnnotationVisitor
    {
        private readonly Recorder _recorder;
        private readonly Record _record;
        private readonly ITraceContext _traceContext;

        public ZipkinAnnotationVisitor(Recorder recorder, Record record, ITraceContext traceContext)
        {
            _recorder = recorder;
            _record = record;
            _traceContext = traceContext;
        }

        public void Visit(ClientRecv clientRecv)
        {
            _recorder.Finish(_traceContext, _record.Timestamp);
        }

        public void Visit(ClientSend clientSend)
        {
            _recorder.Start(_traceContext, _record.Timestamp);
            _recorder.Kind(_traceContext, SpanKind.Client);
        }

        public void Visit(ServerRecv serverRecv)
        {
            _recorder.Start(_traceContext, _record.Timestamp);
            _recorder.Kind(_traceContext, SpanKind.Server);
        }

        public void Visit(ServerSend serverSend)
        {
            _recorder.Finish(_traceContext, _record.Timestamp);
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
            _recorder.Start(_traceContext, _record.Timestamp);
            _recorder.Kind(_traceContext, SpanKind.Producer);
        }

        public void Visit(ProducerStop producerStop)
        {
            _recorder.Finish(_traceContext, _record.Timestamp);
        }

        public void Visit(ConsumerStart consumerStart)
        {
            _recorder.Start(_traceContext, _record.Timestamp);
            _recorder.Kind(_traceContext, SpanKind.Consumer);
        }

        public void Visit(ConsumerStop consumerStop)
        {
            _recorder.Finish(_traceContext, _record.Timestamp);
        }

        public void Visit(MessageAddr messageAddr)
        {
            _recorder.RemoteEndPoint(_traceContext, new EndPoint(messageAddr.ServiceName, messageAddr.Endpoint));
        }

        public void Visit(Event ev)
        {
            _recorder.Annotate(_traceContext, _record.Timestamp, ev.EventName);
        }

        private void AddTimestampedAnnotation(string value)
        {
            _recorder.Annotate(_traceContext, _record.Timestamp, value);
        }

        public void Visit(Rpc rpc)
        {
            _recorder.Name(_traceContext, rpc.Name);
        }

        public void Visit(ServiceName serviceName)
        {
            _recorder.ServiceName(_traceContext, serviceName.Service);
        }

        public void Visit(LocalAddr localAddr)
        {
            _recorder.EndPoint(_traceContext, localAddr.EndPoint);
        }

        public void Visit(LocalOperationStop operation)
        {
            _recorder.Finish(_traceContext, _record.Timestamp);
        }

        public void Visit(LocalOperationStart localOperation)
        {
            _recorder.Start(_traceContext, _record.Timestamp);
            AddBinaryAnnotation(zipkinCoreConstants.LOCAL_COMPONENT, localOperation.OperationName);
        }

        public void Visit(TagAnnotation tagAnnotation)
        {
            AddBinaryAnnotation(tagAnnotation.Key, tagAnnotation.Value);
        }

        public void Visit(ClientAddr clientAddr)
        {
            _recorder.RemoteEndPoint(_traceContext, new EndPoint(null, clientAddr.Endpoint));
        }

        public void Visit(ServerAddr serverAddr)
        {
            _recorder.RemoteEndPoint(_traceContext, new EndPoint(serverAddr.ServiceName, serverAddr.Endpoint));
        }

        private void AddBinaryAnnotation(string annotationKey, object annotationValue)
        {
            _recorder.Tag(_traceContext, _record.Timestamp, annotationKey, annotationValue);
        }
    }
}