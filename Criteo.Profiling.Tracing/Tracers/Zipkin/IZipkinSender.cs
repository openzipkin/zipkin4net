namespace Criteo.Profiling.Tracing.Tracers.Zipkin
{
    public interface IZipkinSender
    {
        void Send(byte[] data);
    }
}
