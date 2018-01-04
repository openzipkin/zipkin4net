using Microsoft.Owin;
using System.Threading.Tasks;
using zipkin4net.Propagation;

namespace zipkin4net.Middleware
{
    class ZipkinMiddleware : OwinMiddleware
    {
        private readonly string serviceName;
        private readonly IExtractor<IHeaderDictionary> traceExtractor;

        public ZipkinMiddleware(
            OwinMiddleware next,
            string serviceName,
            IExtractor<IHeaderDictionary> traceExtractor) : base(next)
        {
            this.serviceName = serviceName;
            this.traceExtractor = traceExtractor;
        }

        public override async Task Invoke(IOwinContext context)
        {
            var traceContext = traceExtractor.Extract(context.Request.Headers);
            var trace = traceContext == null ? Trace.Create() : Trace.CreateFromId(traceContext);

            Trace.Current = trace;

            using (var serverTrace = new ServerTrace(this.serviceName, context.Request.Method))
            {
                trace.Record(Annotations.Tag("http.host", context.Request.Host.Value));
                trace.Record(Annotations.Tag("http.url", context.Request.Uri.AbsoluteUri));
                trace.Record(Annotations.Tag("http.path", context.Request.Uri.AbsolutePath));

                await serverTrace.TracedActionAsync(Next.Invoke(context));
            }
        }
    }
}
