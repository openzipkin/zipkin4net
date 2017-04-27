using System;
using Moq;
using NUnit.Framework;
using OpenTracing;
using Criteo.Profiling.Tracing;
using Criteo.Profiling.Tracing.Annotation;
using Criteo.Profiling.Tracing.Tracers;

namespace Criteo.Profiling.OpenTracing.UTest
{
    [TestFixture]
    internal class T_SpanBuilder
    {
        private const string ServiceName = "service";

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
        public void SpanClientRecordsClientSendAndClientRecv()
        {
            Assert.IsNull(Trace.Current);
            new SpanBuilder(ServiceName)
            .WithTag(Tags.SpanKind, Tags.SpanKindClient)
            .Start()
            .Finish();
            Assert.IsNotNull(Trace.Current);
            TraceManager.Stop();
            Assert.AreEqual(3, _tracer.Records.Count);
            AssertNextAnnotationIs(Annotations.ClientSend());
            AssertNextAnnotationIs(Annotations.ServiceName(ServiceName));
            AssertNextAnnotationIs(Annotations.ClientRecv());
        }

        [Test]
        public void SpanServerRecordsServerRecvAndServerSend()
        {
            Assert.IsNull(Trace.Current);
            new SpanBuilder(ServiceName)
            .WithTag(Tags.SpanKind, Tags.SpanKindServer)
            .Start()
            .Finish();
            Assert.IsNotNull(Trace.Current);
            TraceManager.Stop();
            Assert.AreEqual(3, _tracer.Records.Count);
            AssertNextAnnotationIs(Annotations.ServerRecv());
            AssertNextAnnotationIs(Annotations.ServiceName(ServiceName));
            AssertNextAnnotationIs(Annotations.ServerSend());
        }

        [Test]
        public void SpanWithoutTagKindIsALocalSpan()
        {
            Assert.IsNull(Trace.Current);
            new SpanBuilder(ServiceName)
            .Start()
            .Finish();
            Assert.IsNotNull(Trace.Current);
            TraceManager.Stop();
            Assert.AreEqual(2, _tracer.Records.Count);
            AssertNextAnnotationIs(Annotations.LocalOperationStart(ServiceName));
            AssertNextAnnotationIs(Annotations.LocalOperationStop());
        }
        private void AssertNextAnnotationIs(IAnnotation annotation)
        {
            Utils.AssertNextAnnotationIs(_tracer, annotation);
        }
    }
}