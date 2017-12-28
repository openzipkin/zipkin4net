using System;
using System.Collections.Generic;
using System.Linq;

namespace zipkin4net.Propagation
{
    public class ExtraFieldPropagation
    {
        public static string Get(ITraceContext context, string name)
        {
            if (context == null) throw new NullReferenceException("context == null");
            if (name == null) throw new NullReferenceException("name == null");
            foreach (var elt in context.Extra)
            {
                var extra = elt as Extra;
                if (extra != null)
                {
                    return extra.Get(name);
                }
            }

            return null;
        }

        internal class Extra
        {
            private readonly IDictionary<string, string> _fields = new Dictionary<string, string>();

            internal void Put(string name, string value)
            {
                _fields[name] = value;
            }

            internal string Get(string name)
            {
                string value;
                return _fields.TryGetValue(name, out value) ? value : null;
            }

            public void SetAll<C, K>(C carrier, Setter<C, K> setter, IDictionary<string, K> nameToKey)
            {
                foreach (var field in _fields)
                {
                    K key;
                    if (!nameToKey.TryGetValue(field.Key, out key))
                    {
                        continue;
                    }

                    setter(carrier, key, field.Value);
                }
            }
        }
    }

    public class ExtraFieldPropagation<K> : IPropagation<K>
    {
        private readonly IPropagation<K> _underlyingPropagation;
        private readonly IDictionary<string, K> _nameToKey;

        public ExtraFieldPropagation(IPropagation<K> underlyingPropagation, IEnumerable<string> names,
            KeyFactory<K> keyFactory)
            : this(underlyingPropagation, CreateNameToKey(names, keyFactory))
        {
        }

        private static IDictionary<string, K> CreateNameToKey(IEnumerable<string> names, KeyFactory<K> keyFactory)
        {
            return names.ToDictionary(name => name, name => keyFactory(name));
        }

        private ExtraFieldPropagation(IPropagation<K> underlyingPropagation, IDictionary<string, K> nameToKey)
        {
            _underlyingPropagation = underlyingPropagation;
            _nameToKey = nameToKey;
        }

        public IInjector<C> Injector<C>(Setter<C, K> setter)
        {
            return new ExtraFieldInjector<C, K>(_underlyingPropagation.Injector(setter), setter, _nameToKey);
        }

        public IExtractor<C> Extractor<C>(Getter<C, K> getter)
        {
            return new ExtraFieldExtractor<C, K>(_underlyingPropagation.Extractor(getter), getter, _nameToKey);
        }

        private class ExtraFieldInjector<C, K> : IInjector<C>
        {
            private readonly IInjector<C> _underlyingInjector;
            private readonly Setter<C, K> _setter;
            private readonly IDictionary<string, K> _nameToKey;

            internal ExtraFieldInjector(IInjector<C> underlyingInjector, Setter<C, K> setter,
                IDictionary<string, K> nameToKey)
            {
                _underlyingInjector = underlyingInjector;
                _setter = setter;
                _nameToKey = nameToKey;
            }

            public void Inject(ITraceContext traceContext, C carrier)
            {
                foreach (var elt in traceContext.Extra)
                {
                    var extra = elt as ExtraFieldPropagation.Extra;
                    if (extra != null)
                    {
                        extra.SetAll(carrier, _setter, _nameToKey);
                        break;
                    }
                }

                _underlyingInjector.Inject(traceContext, carrier);
            }
        }

        private class ExtraFieldExtractor<C, K> : IExtractor<C>
        {
            private readonly IExtractor<C> _underlyingExtractor;
            private readonly Getter<C, K> _getter;
            private readonly IDictionary<string, K> _names;

            internal ExtraFieldExtractor(IExtractor<C> underlyingExtractor, Getter<C, K> getter,
                IDictionary<string, K> names)
            {
                _underlyingExtractor = underlyingExtractor;
                _getter = getter;
                _names = names;
            }

            public ITraceContext Extract(C carrier)
            {
                var result = _underlyingExtractor.Extract(carrier);

                var extra = new Lazy<ExtraFieldPropagation.Extra>(() => new ExtraFieldPropagation.Extra());

                foreach (var field in _names)
                {
                    var maybeValue = _getter(carrier, field.Value);
                    if (maybeValue == null) continue;
                    extra.Value.Put(field.Key, maybeValue);
                }

                if (!extra.IsValueCreated) return result;
                var extras = new List<object>();
                extras.AddRange(result.Extra);
                extras.Add(extra.Value);
                return new SpanState(result, extras);
            }
        }
    }
}