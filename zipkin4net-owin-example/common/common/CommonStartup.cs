using Criteo.Profiling.Tracing;
using Criteo.Profiling.Tracing.Tracers.Zipkin;
using Criteo.Profiling.Tracing.Transport.Http;
using Microsoft.Owin.BuilderProperties;
using Owin;
using System.Net.Http;
using System.Threading;
using System.Web.Http;

namespace common
{
    public abstract class CommonStartup
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            HttpConfiguration config = new HttpConfiguration();

            config.Routes.MapHttpRoute(
                name: "Default",
                routeTemplate: GetRouteTemplate(),
                defaults: null,
                constraints: null,
                handler: GetHandler()
            );

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

            // Owin Middleware
            appBuilder.UseZipkinTracer(System.Configuration.ConfigurationManager.AppSettings["applicationName"]);

            appBuilder.UseWebApi(config);
        }

        protected abstract HttpMessageHandler GetHandler();
        protected virtual string GetRouteTemplate() => "";
    }
}
