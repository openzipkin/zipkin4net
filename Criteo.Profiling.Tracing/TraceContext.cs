using System.Runtime.Remoting.Messaging;

namespace Criteo.Profiling.Tracing
{
    internal static class TraceContext
    {
        private const string TraceCallContextKey = "crto_trace";

        public static Trace Get()
        {
            return CallContext.LogicalGetData(TraceCallContextKey) as Trace;
        }

        public static void Set(Trace trace)
        {
            CallContext.LogicalSetData(TraceCallContextKey, trace);
        }

        public static void Clear()
        {
            CallContext.FreeNamedDataSlot(TraceCallContextKey);
        }
    }
}
