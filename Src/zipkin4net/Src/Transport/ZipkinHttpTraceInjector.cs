using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using zipkin4net.Propagation;

namespace zipkin4net.Transport
{
    /**
     * Inject B3 headers into HTTP headers.
     */
    [Obsolete("Please use Propagation.IPropagation instead")]
    public class ZipkinHttpTraceInjector : ITraceInjector<NameValueCollection>, ITraceInjector<IDictionary<string, string>>, ITraceInjector
    {
        private static readonly IInjector<NameValueCollection> NameValueCollectionInjector = Propagations.B3String.Injector<NameValueCollection>((c, key, value) => c[key] = value);
        private static readonly IInjector<IDictionary<string, string>> DictionaryInjector = Propagations.B3String.Injector<IDictionary<string, string>>((c, key, value) => c[key] = value);


        public bool Inject<TE>(Trace trace, TE carrier, Action<TE, string, string> injector)
        {
            var traceContext = trace.CurrentSpan;
            var b3Injector = Propagations.B3String.Injector(new Setter<TE, string>(injector));
            b3Injector.Inject(traceContext, carrier);
            return true;
        }

        public bool Inject(Trace trace, NameValueCollection carrier)
        {
            NameValueCollectionInjector.Inject(trace.CurrentSpan, carrier);
            return true;
        }

        public bool Inject(Trace trace, IDictionary<string, string> carrier)
        {
            DictionaryInjector.Inject(trace.CurrentSpan, carrier);
            return true;
        }
    }
}
