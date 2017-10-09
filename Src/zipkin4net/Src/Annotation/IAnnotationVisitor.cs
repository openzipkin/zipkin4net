namespace zipkin4net.Annotation
{
    public interface IAnnotationVisitor
    {
        void Visit(ClientRecv clientRecv);
        void Visit(ClientSend clientSend);
        void Visit(ServerRecv serverRecv);
        void Visit(ServerSend serverSend);
        void Visit(WireSend wireSend);
        void Visit(WireRecv wireRecv);
        void Visit(ProducerStart producerStart);
        void Visit(ProducerStop producerStop);
        void Visit(ConsumerStart consumerStart);
        void Visit(ConsumerStop consumerStop);
        void Visit(MessageAddr messageAddr);
        void Visit(Rpc rpc);
        void Visit(ServiceName serviceName);
        void Visit(LocalAddr localAddr);
        void Visit(TagAnnotation tagAnnotation);
        void Visit(Event ev);
        void Visit(LocalOperationStart localOperation);
        void Visit(LocalOperationStop operation);
        void Visit(ClientAddr clientAddr);
        void Visit(ServerAddr serverAddr);
    }
}
