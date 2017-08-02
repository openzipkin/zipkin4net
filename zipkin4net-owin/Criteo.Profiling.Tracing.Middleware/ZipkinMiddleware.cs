using Criteo.Profiling.Tracing.Transport;
using Microsoft.Owin;
using System.Threading.Tasks;

namespace Criteo.Profiling.Tracing.Middleware
{
    class ZipkinMiddleware : OwinMiddleware
    {
        private readonly string serviceName;
        private readonly ITraceExtractor traceExtractor;

        public ZipkinMiddleware(
            OwinMiddleware next,
            string serviceName,
            ITraceExtractor traceExtractor) : base(next)
        {
            this.serviceName = serviceName;
            this.traceExtractor = traceExtractor;
        }

        public override async Task Invoke(IOwinContext context)
        {
            Trace trace;

            if (!this.traceExtractor.TryExtract(context.Request.Headers, (dic, k) => string.Join(",", dic[k]), out trace))
            {
                trace = Trace.Create();
            }

            Trace.Current = trace;

            using (var serverTrace = new ServerTrace(this.serviceName, context.Request.Method))
            {
                trace.Record(Annotations.Tag("http.host", context.Request.Host.Value));
                trace.Record(Annotations.Tag("http.uri", context.Request.Uri.AbsoluteUri));
                trace.Record(Annotations.Tag("http.path", context.Request.Uri.AbsolutePath));

                await serverTrace.TracedActionAsync(Next.Invoke(context));
            }
        }
    }
}
