using Criteo.Profiling.Tracing.Middleware;
using Criteo.Profiling.Tracing.Transport;

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
