using zipkin4net;
using zipkin4net.Tracers.Zipkin;
using zipkin4net.Transport.Http;
using Microsoft.Owin;
using Microsoft.Owin.BuilderProperties;
using Owin;
using System.Threading;
using System.Threading.Tasks;

namespace common
{
    public abstract class CommonStartup
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            //Setup tracing
            TraceManager.SamplingRate = 1.0f;
            var logger = new ConsoleLogger();
            var httpSender = new HttpZipkinSender("http://localhost:9411", "application/json");
            var tracer = new ZipkinTracer(httpSender, new JSONSpanSerializer());
            TraceManager.RegisterTracer(tracer);
            TraceManager.Start(logger);
            //

            //Stop TraceManager on app dispose
            var properties = new AppProperties(appBuilder.Properties);
            var token = properties.OnAppDisposing;

            if (token != CancellationToken.None)
            {
                token.Register(() =>
                {
                    TraceManager.Stop();
                });
            }
            //

            // Setup Owin Middleware
            appBuilder.UseZipkinTracer(System.Configuration.ConfigurationManager.AppSettings["applicationName"]);
            //

            appBuilder.Run(RunHandler);
        }

        protected abstract Task RunHandler(IOwinContext context);
    }
}
