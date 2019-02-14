using Microsoft.Owin;
using System;
using zipkin4net.Middleware;
using zipkin4net.Propagation;

namespace Owin
{
    public static class OwinExtensions
    {
        public static IAppBuilder UseZipkinTracer(
            this IAppBuilder appBuilder, 
            string serviceName,
            IExtractor<IHeaderDictionary> traceExtractor, 
            Func<IOwinContext, string> getRpc = null,
            Func<PathString, bool> routeFilter = null)
                => appBuilder.Use<ZipkinMiddleware>(serviceName, traceExtractor, getRpc, routeFilter);

        public static IAppBuilder UseZipkinTracer(
            this IAppBuilder appBuilder, 
            string serviceName,
            Func<IOwinContext, string> getRpc = null,
            Func<PathString, bool> routeFilter = null)
                => appBuilder.UseZipkinTracer(serviceName,
                    Propagations.B3String.Extractor((IHeaderDictionary carrier, string key) =>
                        string.Join(",", carrier[key])), getRpc, routeFilter);
    }
}
