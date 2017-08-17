using System;

namespace Criteo.Profiling.Tracing.Transport
{
    /**
    * Distributed tracing extraction interface
    *
    * Extract a trace from a carrier. It can for example be used
    * to extract B3 headers from HTTP headers to recreate a trace
    * object.
    */
    public interface ITraceExtractor
    {
        bool TryExtract<TE>(TE carrier, Func<TE, string, string> extractor, out Trace trace);
    }

    /**
    * Distributed tracing extraction interface
    *
    * Extract a trace from a carrier. It can for example be used
    * to extract B3 headers from HTTP headers to recreate a trace
    * object.
    */
    public interface ITraceExtractor<in TE>
    {
        bool TryExtract(TE carrier, out Trace trace);
    }
}
