using System;
using OpenTracing;
using OpenTracing.Propagation;
using Criteo.Profiling.Tracing.Transport;

namespace Criteo.Profiling.OpenTracing
{
    public class Tracer : ITracer
    {
        private readonly ITraceInjector _traceInjector;
        private readonly ITraceExtractor _traceExtractor;

        public Tracer()
        : this(new ZipkinHttpTraceInjector(), new ZipkinHttpTraceExtractor())
        {}

        public Tracer(ITraceInjector traceInjector, ITraceExtractor traceExtractor)
        {
            _traceInjector = traceInjector;
            _traceExtractor = traceExtractor;
        }

        public ISpanBuilder BuildSpan(string operationName)
        {
            return new SpanBuilder(operationName);
        }

        public ISpanContext Extract<TCarrier>(Format<TCarrier> format, TCarrier carrier)
        {
            VerifySupportedFormat(format);
            var implCarrier = GetRealCarrier(carrier);
            Criteo.Profiling.Tracing.Trace trace = null;
            if (!_traceExtractor.TryExtract(implCarrier, (c, key) => c.Get(key), out trace))
            {
                return null;
            }
            return new SpanContext(trace);
        }

        public void Inject<TCarrier>(ISpanContext spanContext, Format<TCarrier> format, TCarrier carrier)
        {
            VerifySupportedFormat(format);
            var implCarrier = GetRealCarrier(carrier);
            var trace = GetRealSpanContext(spanContext).Trace;
            _traceInjector.Inject(trace, implCarrier, (c, key, value) => c.Set(key, value));
        }

        private static void VerifySupportedFormat<TCarrier>(Format<TCarrier> format)
        {
            if (!Formats.HttpHeaders.Name.Equals(format.Name) && !Formats.TextMap.Name.Equals(format.Name))
            {
                throw new UnsupportedFormatException("Format " + format.ToString() + " not supported");
            }
        }

        private static ITextMap GetRealCarrier<TCarrier>(TCarrier carrier)
        {
            if (carrier == null)
            {
                throw new NullReferenceException("Carrier can't be null");
            }
            var implCarrier = carrier as ITextMap;
            if (implCarrier == null)
            {
                throw new NotSupportedException("Carriers other than ITextMap are not supported.");
            }
            return implCarrier;
        }

        private static SpanContext GetRealSpanContext(ISpanContext spanContext)
        {
            if (spanContext == null)
            {
                throw new NullReferenceException("SpanContext can't be null");
            } 
            var impl = spanContext as SpanContext;
            if (impl == null)
            {
                throw new NotSupportedException("You must provide the library with SpanContext created by the itself.");
            }
            return impl;
        }
    }
}