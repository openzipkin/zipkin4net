using Microsoft.Owin;
using System;
using System.Threading.Tasks;
using zipkin4net.Propagation;

namespace zipkin4net.Middleware
{
    class ZipkinMiddleware : OwinMiddleware
    {
        private readonly string serviceName;
        private readonly IExtractor<IHeaderDictionary> traceExtractor;
        private readonly Func<IOwinContext, string> getRpc;
        private readonly Func<PathString, bool> routeFilter;

        public ZipkinMiddleware(
            OwinMiddleware next,
            string serviceName,
            IExtractor<IHeaderDictionary> traceExtractor,
            Func<IOwinContext, string> getRpc = null,
            Func<PathString, bool> routeFilter = null) : base(next)
        {
            this.serviceName = serviceName;
            this.traceExtractor = traceExtractor;
            this.getRpc = getRpc ?? (context => context.Request.Method);
            this.routeFilter = routeFilter ?? (x => true);
        }

        public override async Task Invoke(IOwinContext context)
        {
            var traceContext = traceExtractor.Extract(context.Request.Headers);
            var trace = traceContext == null ? Trace.Create() : Trace.CreateFromId(traceContext);

            Trace.Current = trace;

            if (routeFilter(context.Request.Path))
            {
                using (var serverTrace = new ServerTrace(this.serviceName, this.getRpc(context)))
                {
                    trace.Record(Annotations.Tag("http.host", context.Request.Host.Value));
                    trace.Record(Annotations.Tag("http.url", context.Request.Uri.AbsoluteUri));
                    trace.Record(Annotations.Tag("http.path", context.Request.Uri.AbsolutePath));

                    await serverTrace.TracedActionAsync(Next.Invoke(context));
                }
            }
        }
    }
}
