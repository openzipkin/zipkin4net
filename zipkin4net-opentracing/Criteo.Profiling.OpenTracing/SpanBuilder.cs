using System;
using System.Collections.Generic;
using System.Globalization;
using Criteo.Profiling.Tracing;
using Criteo.Profiling.Tracing.Annotation;
using OpenTracing;

namespace Criteo.Profiling.OpenTracing
{
    public class SpanBuilder : ISpanBuilder
    {
        private readonly string _operationName;

        private readonly IDictionary<string, string> _tags = new Dictionary<string, string>();
        private DateTime _startTimestamp;
        private SpanContext _parent;

        public SpanBuilder(string operationName)
        {
            _operationName = operationName;
        }

        public ISpanBuilder AddReference(string referenceType, ISpanContext referencedContext)
        {
            if (_parent != null)
            {
                return this;
            }
            if (References.ChildOf.Equals(referenceType) || References.FollowsFrom.Equals(referenceType)) {
                _parent = (SpanContext) referencedContext;
            }
            return this;
        }

        public ISpanBuilder AsChildOf(ISpan parent)
        {
            return AsChildOf(parent.Context);
        }

        public ISpanBuilder AsChildOf(ISpanContext parent)
        {
            return AddReference(References.ChildOf, parent);
        }

        public ISpanBuilder FollowsFrom(ISpan parent)
        {
            return AddReference(References.FollowsFrom, parent.Context);
        }

        public ISpanBuilder FollowsFrom(ISpanContext parent)
        {
            return AddReference(References.FollowsFrom, parent);
        }

        public ISpan Start()
        {
            var spanKind = GetSpanKind(_tags);
            var trace = GetCurrentTraceAccordingToSpanKind(spanKind);
            Trace.Current = trace;
            // Forcing sampling on child spans would lead to inconsistent data.
            if (_parent == null)
            {
                var forceSampling = IsSamplingForced(_tags);
                if (forceSampling)
                {
                    trace.ForceSampled();
                }
            }
            
            var annotation = GetOpeningAnnotation(spanKind, _operationName);
            if (_startTimestamp != default(DateTime))
            {
                trace.Record(annotation, _startTimestamp);
            }
            else
            {
                trace.Record(annotation);
            }
            if (_operationName != null && spanKind != Span.SpanKind.Local)
            {
                trace.Record(Annotations.ServiceName(_operationName));
            }
            foreach (var entry in _tags)
            {
                if (entry.Key.Equals(Tags.SpanKind))
                {
                    continue;
                }
                trace.Record(Annotations.Tag(entry.Key, entry.Value));
            }
            return new Span(trace, spanKind);
        }

        private static Trace GetCurrentTraceAccordingToSpanKind(Span.SpanKind spanKind)
        {
            var trace = Trace.Current;
            switch (spanKind)
            {
                case Span.SpanKind.Server:
                {
                    if (trace == null)
                    {
                        trace = Trace.Create();
                        Trace.Current = trace;
                    }
                    break;
                }
                case Span.SpanKind.Client:
                case Span.SpanKind.Local:
                {
                    trace = (trace == null ? Trace.Create() : trace.Child());
                    Trace.Current = trace;
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return trace;
        }

        private static bool IsSamplingForced(IDictionary<string, string> tags)
        {
            string sampling;
            if (tags.TryGetValue(Tags.SamplingPriority, out sampling) && sampling != default(string))
            {
                try
                {
                    var samplingPriority = int.Parse(sampling);
                    return samplingPriority > 0;
                }
                catch (FormatException)
                {
                    return false;
                }
            }
            return false;
        }

        private static Span.SpanKind GetSpanKind(IDictionary<string, string> tags)
        {
            string spanKind;
            if (tags.TryGetValue(Tags.SpanKind, out spanKind))
            {
                return Tags.SpanKindClient.Equals(spanKind) ? Span.SpanKind.Client : Span.SpanKind.Server;
            }
            else
            {
                return Span.SpanKind.Local;
            }
        }

        public ISpanBuilder WithStartTimestamp(DateTime startTimestamp)
        {
            _startTimestamp = startTimestamp;
            return this;
        }

        public ISpanBuilder WithTag(string key, bool value)
        {
            return WithTag(key, Convert.ToString(value));
        }

        public ISpanBuilder WithTag(string key, double value)
        {
            return WithTag(key, Convert.ToString(value, CultureInfo.InvariantCulture));
        }

        public ISpanBuilder WithTag(string key, int value)
        {
            return WithTag(key, Convert.ToString(value));
        }

        public ISpanBuilder WithTag(string key, string value)
        {
            _tags.Add(key, value);
            return this;
        }

        private static IAnnotation GetOpeningAnnotation(Span.SpanKind spanKind, string operationName)
        {
            switch (spanKind)
            {
                case Span.SpanKind.Client:
                    return Annotations.ClientSend();
                case Span.SpanKind.Server:
                    return Annotations.ServerRecv();
                case Span.SpanKind.Local:
                {
                    if (operationName == null)
                    {
                        throw new NullReferenceException("Trying to start a local span without any operation name is forbidden");
                    }
                    return Annotations.LocalOperationStart(operationName);
                }
            }
            throw new NotSupportedException("SpanKind: " + spanKind + " unknown.");
        }
    }
}