namespace Criteo.Profiling.Tracing.Annotation
{
    public interface IAnnotationVisitor
    {
        void Visit(ClientRecv clientRecv);
        void Visit(ClientSend clientSend);
        void Visit(ServerRecv serverRecv);
        void Visit(ServerSend serverSend);
        void Visit(WireSend wireSend);
        void Visit(WireRecv wireRecv);
        void Visit(Rpc rpc);
        void Visit(ServiceName serviceName);
        void Visit(LocalAddr localAddr);
        void Visit(TagAnnotation tagAnnotation);
        void Visit(Event ev);
    }
}
