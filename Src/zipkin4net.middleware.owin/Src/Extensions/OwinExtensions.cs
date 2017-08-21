using zipkin4net.Middleware;
using zipkin4net.Transport;

namespace Owin
{
    public static class OwinExtensions
    {
        public static IAppBuilder UseZipkinTracer(this IAppBuilder appBuilder, string serviceName, ITraceExtractor traceExtractor)
            => appBuilder.Use<ZipkinMiddleware>(serviceName, traceExtractor);

        public static IAppBuilder UseZipkinTracer(this IAppBuilder appBuilder, string serviceName)
            => appBuilder.UseZipkinTracer(serviceName, new ZipkinHttpTraceExtractor());
    }
}
