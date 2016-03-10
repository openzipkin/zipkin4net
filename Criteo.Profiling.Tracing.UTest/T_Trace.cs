using Criteo.Profiling.Tracing.Dispatcher;
using Moq;
using NUnit.Framework;

namespace Criteo.Profiling.Tracing.UTest
{
    [TestFixture]
    class T_Trace
    {
        [SetUp]
        public void SetUp()
        {
            TraceManager.ClearTracers();
            TraceManager.Stop();
        }

        [Test]
        public void TracerRecordShouldBeCalledIfTracingIsStarted()
        {
            var trace = Trace.Create();

            var mockTracer = new Mock<ITracer>();
            TraceManager.RegisterTracer(mockTracer.Object);

            TraceManager.Start(new Configuration());
            Assert.IsTrue(TraceManager.Started);

            trace.Record(Annotations.ServerSend());
            TraceManager.Stop();

            mockTracer.Verify(tracer => tracer.Record(It.IsAny<Record>()), Times.Once());
        }

        [Test]
        public void RecordsShouldntBeSentToTracersIfTracingIsStopped()
        {
            var trace = Trace.Create();

            var mockTracer = new Mock<ITracer>();
            TraceManager.RegisterTracer(mockTracer.Object);

            Assert.IsFalse(TraceManager.Started);

            trace.Record(Annotations.ServerSend());

            mockTracer.Verify(tracer => tracer.Record(It.IsAny<Record>()), Times.Never());
        }

        [Test]
        public void ChildTraceIsCorrectlyCreated()
        {
            var parent = Trace.Create();
            var child = parent.Child();

            // Should share the same global id
            Assert.AreEqual(parent.CurrentSpan.TraceId, child.CurrentSpan.TraceId);
            Assert.AreEqual(parent.CorrelationId, child.CorrelationId);

            // Parent id of the child should be the parent span id
            Assert.AreEqual(parent.CurrentSpan.SpanId, child.CurrentSpan.ParentSpanId);

            // Flags should be copied
            Assert.AreEqual(parent.CurrentSpan.Flags, child.CurrentSpan.Flags);

            // Child cannot have the same span id
            Assert.AreNotEqual(parent.CurrentSpan.SpanId, child.CurrentSpan.SpanId);
        }

        [Test]
        public void TraceCreatesCorrectRecord()
        {
            var trace = Trace.Create();

            var dispatcher = new Mock<IRecordDispatcher>();
            TraceManager.Start(new Configuration(), dispatcher.Object);

            var clientRcv = Annotations.ClientRecv();
            trace.Record(clientRcv);

            dispatcher.Verify(d => d.Dispatch(It.Is<Record>(r => r.Annotation == clientRcv && r.SpanState.Equals(trace.CurrentSpan))), Times.Once());
        }


        [Test]
        public void TraceSamplingForced()
        {
            TraceManager.Sampler.SamplingRate = 0.0f;
            var trace = Trace.Create();

            Assert.True(trace.CurrentSpan.Flags.HasFlag(SpanFlags.SamplingKnown));
            Assert.False(trace.CurrentSpan.Flags.HasFlag(SpanFlags.Sampled));
            trace.ForceSampled();
            Assert.True(trace.CurrentSpan.Flags.HasFlag(SpanFlags.Sampled));
        }

    }
}
