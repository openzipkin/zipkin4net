using zipkin4net;
using zipkin4net.Tracers.Zipkin;
using zipkin4net.Transport.Http;

namespace example.message.common
{
    public static class ZipkinHelper
    {
        public static void StartZipkin(string zipkinServer)
        {
            TraceManager.SamplingRate = 1.0f;
            var httpSender = new HttpZipkinSender(zipkinServer, "application/json");
            var tracer = new ZipkinTracer(httpSender, new JSONSpanSerializer());
            TraceManager.RegisterTracer(tracer);
            TraceManager.Start(new ZipkinConsoleLogger());
        }

        public static void StopZipkin()
        {
            TraceManager.Stop();
        }
    }
}
