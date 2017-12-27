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
        private class ZipkinHttpTraceInjectorSetter<C> : ISetter<C, string>
        {
            private readonly Action<C, string, string> _injector;

            internal ZipkinHttpTraceInjectorSetter(Action<C, string, string> injector)
            {
                _injector = injector;
            }

            public void Put(C carrier, string key, string value)
            {
                _injector(carrier, key, value);
            }
        }

        private static ISetter<NameValueCollection, string> NameValueCollectionSetter = new ZipkinHttpTraceInjectorSetter<NameValueCollection>((c, key, value) => c[key] = value);
        private static ISetter<IDictionary<string, string>, string> DictionarySetter = new ZipkinHttpTraceInjectorSetter<IDictionary<string, string>>((c, key, value) => c[key] = value);

        private static readonly IInjector<NameValueCollection> NameValueCollectionInjector = Propagations.B3String.Injector(NameValueCollectionSetter);
        private static readonly IInjector<IDictionary<string, string>> DictionaryInjector = Propagations.B3String.Injector(DictionarySetter);


        public bool Inject<TE>(Trace trace, TE carrier, Action<TE, string, string> injector)
        {
            var traceContext = trace.CurrentSpan;
            var b3Injector = Propagations.B3String.Injector(new ZipkinHttpTraceInjectorSetter<TE>(injector));
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
