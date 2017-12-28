using Microsoft.Owin;
using zipkin4net.Middleware;
using zipkin4net.Propagation;

namespace Owin
{
    public static class OwinExtensions
    {
        public static IAppBuilder UseZipkinTracer(this IAppBuilder appBuilder, string serviceName, IExtractor<IHeaderDictionary> traceExtractor)
            => appBuilder.Use<ZipkinMiddleware>(serviceName, traceExtractor);

        public static IAppBuilder UseZipkinTracer(this IAppBuilder appBuilder, string serviceName)
            => appBuilder.UseZipkinTracer(serviceName, Propagations.B3String.Extractor(new HeaderDictionaryGetter()));

        private class HeaderDictionaryGetter : IGetter<IHeaderDictionary, string>
        {
            public string Get(IHeaderDictionary carrier, string key)
            {
                return string.Join(",", carrier[key]);
            }
        }
    }
}
