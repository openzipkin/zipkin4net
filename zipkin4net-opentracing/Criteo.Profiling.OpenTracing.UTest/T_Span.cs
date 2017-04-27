using System;
using Criteo.Profiling.Tracing;
using Criteo.Profiling.Tracing.Annotation;
using Criteo.Profiling.Tracing.Tracers;
using Moq;
using NUnit.Framework;

namespace Criteo.Profiling.OpenTracing.UTest
{
    [TestFixture]
    class T_Span
    {
        private InMemoryTracer _tracer;

        [SetUp]
        public void SetUp()
        {
            Trace.Current = null;
            _tracer = new InMemoryTracer();
            TraceManager.RegisterTracer(_tracer);
            TraceManager.SamplingRate = 1.0f;
            TraceManager.Start(Mock.Of<ILogger>());
        }

        [TearDown]
        public void TearDown()
        {
            TraceManager.ClearTracers();
        }

        [Test]
        public void DisposePatternAfterFinishDoesntReportTwice()
        {
            var trace = Trace.Create();
            using (var span = new Span(trace, Span.SpanKind.Server))
            {
                span.Finish();
            }
            TraceManager.Stop();
            Assert.AreEqual(1, _tracer.Records.Count); //Only one closing annotation
        }

        [Test]
        public void SetTagShouldRecord()
        {
            var trace = Trace.Create();
            var span = new Span(trace, Span.SpanKind.Server);
            const string key = "key";
            const string value = "value";
            span.SetTag(key, value);
            TraceManager.Stop();
            Assert.AreEqual(1, _tracer.Records.Count);
            Utils.AssertNextAnnotationIs(_tracer, Annotations.Tag(key, value));
        }

        [Test]
        public void LogShouldRecord()
        {
            var trace = Trace.Create();
            var span = new Span(trace, Span.SpanKind.Server);
            const string log = "event";
            span.Log(log);
            TraceManager.Stop();
            Assert.AreEqual(1, _tracer.Records.Count);
            Utils.AssertNextAnnotationIs(_tracer, Annotations.Event(log));
        }

        [Test]
        public void LogWithTimestampShouldRecord()
        {
            var trace = Trace.Create();
            var span = new Span(trace, Span.SpanKind.Server);
            const string log = "event";
            var timestamp = DateTime.UtcNow;
            span.Log(timestamp, log);
            TraceManager.Stop();
            Assert.AreEqual(1, _tracer.Records.Count);
            Utils.AssertNextAnnotationIs(_tracer, Annotations.Event(log), timestamp);
        }
    }
}
