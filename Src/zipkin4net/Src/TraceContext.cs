using System;
#if NET_CORE
using System.Threading;
#else
using System.Runtime.Remoting.Messaging;
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

            return CallContext.LogicalGetData(TraceCallContextKey) as Trace;
#endif
        }

        public static void Set(Trace trace)
        {
#if NET_CORE
            AsyncLocalTrace.Value = trace;
#else
            if (IsRunningOnMono) return;

            CallContext.LogicalSetData(TraceCallContextKey, trace);
#endif
        }

        public static void Clear()
        {
#if NET_CORE
            AsyncLocalTrace.Value = null;
#else
            if (IsRunningOnMono) return;

            CallContext.FreeNamedDataSlot(TraceCallContextKey);
#endif
        }
    }
}
