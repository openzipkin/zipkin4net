using System;
using System.Runtime.Remoting.Messaging;

namespace Criteo.Profiling.Tracing
{
    internal static class TraceContext
    {
        private const string TraceCallContextKey = "crto_trace";

        private static readonly bool IsRunningOnMono = (Type.GetType("Mono.Runtime") != null);

        public static Trace Get()
        {
            if (IsRunningOnMono) return null;

            return CallContext.LogicalGetData(TraceCallContextKey) as Trace;
        }

        public static void Set(Trace trace)
        {
            if (IsRunningOnMono) return;

            CallContext.LogicalSetData(TraceCallContextKey, trace);
        }

        public static void Clear()
        {
            if (IsRunningOnMono) return;

            CallContext.FreeNamedDataSlot(TraceCallContextKey);
        }
    }
}
