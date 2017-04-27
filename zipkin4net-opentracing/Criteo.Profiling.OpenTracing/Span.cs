using System;
using System.Collections.Generic;
using System.Globalization;
using Criteo.Profiling.Tracing;
using Criteo.Profiling.Tracing.Annotation;
using OpenTracing;
using System.Linq;

namespace Criteo.Profiling.OpenTracing
{
    internal class Span : ISpan
    {
        public ISpanContext Context { get; }

        internal enum SpanKind
        {
            Server,
            Client,
            Local
        }

        private readonly Trace _trace;
        private readonly SpanKind _spanKind;

        internal Span(Trace trace, SpanKind spanKind)
        {
            _trace = trace;
            _spanKind = spanKind;
            Context = new SpanContext(trace);
        }

        public ISpan Log(IEnumerable<KeyValuePair<string, object>> fields)
        {
            _trace.Record(Annotations.Event(JoinKeyValuePairs(fields)));
            return this;
        }

        public ISpan Log(DateTime timestamp, IEnumerable<KeyValuePair<string, object>> fields)
        {
            _trace.Record(Annotations.Event(JoinKeyValuePairs(fields)), timestamp);
            return this;
        }

        public ISpan Log(string eventName)
        {
            _trace.Record(Annotations.Event(eventName));
            return this;
        }

        public ISpan Log(DateTime timestamp, string eventName)
        {
            _trace.Record(Annotations.Event(eventName), timestamp);
            return this;
        }

        public string GetBaggageItem(string key)
        {
            throw new NotSupportedException();
        }

        public ISpan SetBaggageItem(string key, string value)
        {
            throw new NotSupportedException();
        }

        public ISpan SetOperationName(string operationName)
        {
            _trace.Record(Annotations.ServiceName(operationName));
            return this;
        }

        public ISpan SetTag(string key, bool value)
        {
            return SetTag(key, Convert.ToString(value));
        }

        public ISpan SetTag(string key, double value)
        {
            return SetTag(key, Convert.ToString(value, CultureInfo.InvariantCulture));
        }

        public ISpan SetTag(string key, int value)
        {
            return SetTag(key, Convert.ToString(value));
        }

        public ISpan SetTag(string key, string value)
        {
            _trace.Record(Annotations.Tag(key, value));
            return this;
        }

        private static string JoinKeyValuePairs(IEnumerable<KeyValuePair<string, object>> fields)
        {
            return string.Join(" ", fields.Select(entry => entry.Key + ":" + entry.Value));
        }

        public void Finish()
        {
            if (!_isFinished)
            {
                _trace.Record(GetClosingAnnotation(_spanKind));
                _isFinished = true;
            }
        }

        public void Finish(DateTime finishTimestamp)
        {
            if (!_isFinished)
            {
                _trace.Record(GetClosingAnnotation(_spanKind), finishTimestamp);
                _isFinished = true;
            }
        }

        private static IAnnotation GetClosingAnnotation(SpanKind spanKind)
        {
            switch (spanKind)
            {
                case SpanKind.Client:
                    return Annotations.ClientRecv();
                case SpanKind.Server:
                    return Annotations.ServerSend();
                case SpanKind.Local:
                    return Annotations.LocalOperationStop();
            }
            throw new NotSupportedException("SpanKind: " + spanKind + " unknown.");
        }

        private bool _isFinished = false; // To detect redundant calls


        public void Dispose()
        {
            if (!_isFinished)
            {
                Finish();
                _isFinished = true;
            }
        }
    }
}