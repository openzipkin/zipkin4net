using System;

namespace Criteo.Profiling.Tracing.Transport
{
    /**
    * Distributed tracing injection interface.
    *
    * Inject into a carrier informations needed to propagate
    * a trace. It can for example be used to propagate
    * B3 headers in http headers.
    */
    public interface ITraceInjector
    {
        bool Inject<TE>(Trace trace, TE carrier, Action<TE, string, string> injector);
    }

    /**
    * Distributed tracing injection interface.
    *
    * Inject into a carrier informations needed to propagate
    * a trace. It can for example be used to propagate
    * B3 headers in http headers.
    */
    public interface ITraceInjector<in TE>
    {
        bool Inject(Trace trace, TE carrier);
    }
}
