using System;
using Moq;
using NUnit.Framework;
using Criteo.Profiling.Tracing;
using Criteo.Profiling.Tracing.Transport;
using Criteo.Profiling.OpenTracing;
using OpenTracing;
using OpenTracing.Propagation;


namespace Criteo.Profiling.OpenTracing.UTest
{
    [TestFixture]
    internal class T_Tracer
    {

        private Mock<ITraceInjector> injector = new Mock<ITraceInjector>();
        private Mock<ITraceExtractor> extractor = new Mock<ITraceExtractor>();

        [Test]
        public void ExtractorShouldNotBeCalledIfFormatUnsupported()
        {
            var tracer = new Tracer(injector.Object, extractor.Object);
            Assert.Throws<UnsupportedFormatException>(() => tracer.Extract(new Format<ITextMap>("test"), Mock.Of<ITextMap>()));
        }

        [Test]
        public void ExtractorShouldNotBeCalledIfCarrierUnsupported()
        {
            var tracer = new Tracer(injector.Object, extractor.Object);
            Assert.Throws<NotSupportedException>(() => tracer.Extract(new Format<ISpanContext>(Formats.HttpHeaders.Name), Mock.Of<ISpanContext>()));
        }

        [Test]
        public void ExtractorShouldNotBeCalledIfCarrierIsNull()
        {
            var tracer = new Tracer(injector.Object, extractor.Object);
            ITextMap carrier = null;
            Assert.Throws<NullReferenceException>(() => tracer.Extract(Formats.HttpHeaders, carrier));
        }

        [Test]
        public void InjectorShouldNotBeCalledIfFormatUnsupported()
        {
            var tracer = new Tracer(injector.Object, extractor.Object);
            Assert.Throws<UnsupportedFormatException>(() => tracer.Inject(Mock.Of<ISpanContext>(), new Format<ITextMap>("test"), Mock.Of<ITextMap>()));
        }

        [Test]
        public void InjectorShouldNotBeCalledIfCarrierUnsupported()
        {
            var tracer = new Tracer(injector.Object, extractor.Object);
            Assert.Throws<NotSupportedException>(() => tracer.Inject(Mock.Of<ISpanContext>(), new Format<ISpanContext>(Formats.HttpHeaders.Name), Mock.Of<ISpanContext>()));
        }

        [Test]
        public void InjectorShouldNotBeCalledIfCarrierIsNull()
        {
            var tracer = new Tracer(injector.Object, extractor.Object);
            ITextMap carrier = null;
            Assert.Throws<NullReferenceException>(() => tracer.Inject(Mock.Of<ISpanContext>(), Formats.HttpHeaders, carrier));
        }

        [Test]
        public void InjectorShouldNotBeCalledIfSpanContextIsNull()
        {
            var tracer = new Tracer(injector.Object, extractor.Object);
            ISpanContext context = null;
            Assert.Throws<NullReferenceException>(() => tracer.Inject(context, Formats.HttpHeaders, Mock.Of<ITextMap>()));
        }

        [Test]
        public void InjectorShouldNotBeCalledIfSpanContextIsNotFromTheLibrary()
        {
            var tracer = new Tracer(injector.Object, extractor.Object);
            ISpanContext context = Mock.Of<ISpanContext>();
            Assert.Throws<NotSupportedException>(() => tracer.Inject(context, Formats.HttpHeaders, Mock.Of<ITextMap>()));
        }

        [Test]
        public void InjectorShouldBeCalledWithTraceAndCarrier()
        {
            var tracer = new Tracer(injector.Object, extractor.Object);
            var trace = Trace.Create();
            ISpanContext context = new SpanContext(trace);
            ITextMap carrier = Mock.Of<ITextMap>();
            tracer.Inject(context, Formats.HttpHeaders, carrier);
            injector.Verify(i => i.Inject(trace, carrier, It.IsAny<Action<ITextMap, string, string>>()));
        }

        [Test]
        public void ExtractorShouldBeCalledWithCarrier()
        {
            var tracer = new Tracer(injector.Object, extractor.Object);
            Trace trace = null;
            ISpanContext context = new SpanContext(trace);
            ITextMap carrier = Mock.Of<ITextMap>();
            tracer.Extract(Formats.HttpHeaders, carrier);
            extractor.Verify(e => e.TryExtract(carrier, It.IsAny<Func<ITextMap, string, string>>(), out trace));
        }
    }
}
