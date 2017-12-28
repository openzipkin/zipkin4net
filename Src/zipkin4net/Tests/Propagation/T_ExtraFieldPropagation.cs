using System.Collections.Generic;
using NUnit.Framework;
using zipkin4net.Propagation;

namespace zipkin4net.UTest.Propagation
{
    [TestFixture]
    public class T_ExtraFieldPropagation
    {
        private IDictionary<string, string> _carrier;

        private const string Key1 = "key1";
        private const string Key2 = "key2";
        private const string Key1Value = "value1";
        private const string Key2Value = "value2";

        private readonly ITraceContext _context = new SpanState(1L, null, 2L, true, false);

        private static readonly ExtraFieldPropagation<string> Propagation =
            new ExtraFieldPropagation<string>(Propagations.B3String,
                new List<string>
                {
                    Key1,
                    Key2
                }, KeyFactories.String);

        private static readonly IExtractor<IDictionary<string, string>> Extractor =
            Propagation.Extractor<IDictionary<string, string>>((carrier, key) =>
            {
                string value;
                if (carrier.TryGetValue(key, out value))
                {
                    return value;
                }

                return null;
            });

        private static readonly IInjector<IDictionary<string, string>> Injector =
            Propagation.Injector<IDictionary<string, string>>((carrier, key, value) => carrier[key] = value);

        [SetUp]
        public void SetUp()
        {
            _carrier = new Dictionary<string, string>();
        }

        [Test]
        public void GetShouldReturnExtraIfPresent()
        {
            var context = ContextWithKey1();

            Assert.AreEqual(Key1Value,
                ExtraFieldPropagation.Get(context, Key1));
        }

        [Test]
        public void GetShouldReturnNullIfNoExtraPresent()
        {
            Assert.IsNull(ExtraFieldPropagation.Get(_context, Key1));
        }

        [Test]
        public void ExtraFieldShouldBeInjectedInCarrier()
        {
            var extra = new ExtraFieldPropagation.Extra();
            extra.Put(Key1, Key1Value);
            var context = new SpanState(_context, new List<object> {extra});

            Injector.Inject(context, _carrier);

            Assert.True(_carrier.Contains(new KeyValuePair<string, string>(Key1, Key1Value)));
        }

        [Test]
        public void MultipleExtraFieldsShouldBeInjectedInCarrier()
        {
            var extra = new ExtraFieldPropagation.Extra();
            extra.Put(Key1, Key1Value);
            extra.Put(Key2, Key2Value);
            var context = new SpanState(_context, new List<object> {extra});

            Injector.Inject(context, _carrier);

            Assert.True(_carrier.Contains(new KeyValuePair<string, string>(Key1, Key1Value)));
            Assert.True(_carrier.Contains(new KeyValuePair<string, string>(Key2, Key2Value)));
        }

        [Test]
        public void ExtraFieldShouldBeExtractedFromCarrier()
        {
            Injector.Inject(_context, _carrier);
            _carrier[Key1] = Key1Value;

            var extracted = Extractor.Extract(_carrier);

            Assert.AreEqual(1, extracted.Extra.Count);
            Assert.IsAssignableFrom<ExtraFieldPropagation.Extra>(extracted.Extra[0]);
            Assert.AreEqual(Key1Value, ((ExtraFieldPropagation.Extra) extracted.Extra[0]).Get(Key1));
        }

        [Test]
        public void MultipleExtraFieldsShouldBeExtractedFromCarrier()
        {
            Injector.Inject(_context, _carrier);
            _carrier[Key1] = Key1Value;
            _carrier[Key2] = Key2Value;

            var extracted = Extractor.Extract(_carrier);

            Assert.AreEqual(1, extracted.Extra.Count);
            var extra = extracted.Extra[0];
            Assert.IsAssignableFrom<ExtraFieldPropagation.Extra>(extra);
            Assert.AreEqual(Key1Value, ((ExtraFieldPropagation.Extra) extra).Get(Key1));
            Assert.AreEqual(Key2Value, ((ExtraFieldPropagation.Extra) extra).Get(Key2));
        }

        private ITraceContext ContextWithKey1()
        {
            Injector.Inject(_context, _carrier);

            _carrier[Key1] = Key1Value;
            return Extractor.Extract(_carrier);
        }
    }
}