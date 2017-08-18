namespace zipkin4net.Tracers.Zipkin
{
    public interface IZipkinSender
    {
        void Send(byte[] data);
    }
}
