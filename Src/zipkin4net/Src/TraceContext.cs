using System;
#if NET_CORE
using System.Threading;
#else
using System.Runtime.Remoting.Messaging;
#endif
#if NETFULL
using System.Web;
#endif

namespace zipkin4net
{
    internal static class TraceContext
    {
        private const string TraceCallContextKey = "crto_trace";

#if NET_CORE
        private static readonly AsyncLocal<Trace> AsyncLocalTrace = new AsyncLocal<Trace>();
#endif

        private static readonly bool IsRunningOnMono = Type.GetType("Mono.Runtime") != null;

        public static Trace Get()
        {
#if NET_CORE
            return AsyncLocalTrace.Value;
#else
            if (IsRunningOnMono) return null;

            var trace = CallContext.LogicalGetData(TraceCallContextKey) as Trace;

            // For applications hosted in IIS, the controller call context is not shared with the call context in which the middleware is executed.
            // https://stackoverflow.com/questions/29194836/passing-logical-call-context-from-owin-pipeline-to-webapi-controller
            // This means that the Trace set in the OWIN middleware cannot be retrieved by the TracingHandler.
            if (trace == null)
            {
#if NETFULL 
                return HttpContext.Current?.Items[TraceCallContextKey] as Trace;
#endif
            }

            return trace;
#endif
        }

        public static void Set(Trace trace)
        {
#if NET_CORE
            AsyncLocalTrace.Value = trace;
#else
            if (IsRunningOnMono) return;

            CallContext.LogicalSetData(TraceCallContextKey, trace);

#if NETFULL
            if (HttpContext.Current != null)
            {
                HttpContext.Current.Items[TraceCallContextKey] = trace;
            }
#endif
#endif
        }

        public static void Clear()
        {
#if NET_CORE
            AsyncLocalTrace.Value = null;
#else
            if (IsRunningOnMono) return;

            CallContext.FreeNamedDataSlot(TraceCallContextKey);

#if NETFULL
            if (HttpContext.Current != null)
            {
                HttpContext.Current.Items[TraceCallContextKey] = null;
            }
#endif
#endif
        }
    }
}
