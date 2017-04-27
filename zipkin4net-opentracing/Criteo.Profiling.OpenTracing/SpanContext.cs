using OpenTracing;
using Criteo.Profiling.Tracing;
using System;
using System.Collections.Generic;

namespace Criteo.Profiling.OpenTracing
{
    internal class SpanContext : ISpanContext
    {
        internal Trace Trace { get; }

        internal SpanContext(Trace trace)
        {
            Trace = trace;
        }

        public IEnumerable<KeyValuePair<string, string>> GetBaggageItems()
        {
            throw new NotSupportedException();
        }
    }
}