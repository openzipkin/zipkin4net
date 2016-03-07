using Criteo.Profiling.Tracing.Dispatcher;
using Moq;
using NUnit.Framework;

namespace Criteo.Profiling.Tracing.UTest
{
    [TestFixture]
    class T_Trace
    {

        private readonly SpanState _spanState = new SpanState(1, null, 2, SpanFlags.None);

        [SetUp]
        public void SetUp()
        {
            TraceManager.ClearTracers();
            TraceManager.Stop();
        }

        [Test]
        public void TracerRecordShouldBeCalledIfTracingIsStarted()
        {
            var trace = Trace.CreateFromId(_spanState);

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
            var trace = Trace.CreateFromId(_spanState);

            var mockTracer = new Mock<ITracer>();
            TraceManager.RegisterTracer(mockTracer.Object);

            Assert.IsFalse(TraceManager.Started);

            trace.Record(Annotations.ServerSend());

            mockTracer.Verify(tracer => tracer.Record(It.IsAny<Record>()), Times.Never());
        }

        [Test]
        public void ChildTraceIsCorrectlyCreated()
        {
            var parent = Trace.CreateFromId(_spanState);
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
            var trace = Trace.CreateFromId(_spanState);

            var dispatcher = new Mock<IRecordDispatcher>();
            TraceManager.Start(new Configuration(), dispatcher.Object);

            var clientRcv = Annotations.ClientRecv();
            trace.Record(clientRcv);

            dispatcher.Verify(d => d.Dispatch(It.Is<Record>(r => r.Annotation == clientRcv && r.SpanState.Equals(trace.CurrentSpan))), Times.Once());
        }


        [Test]
        public void TraceSamplingForced()
        {
            var trace = Trace.CreateFromId(_spanState);

            Assert.AreEqual(SamplingStatus.NoDecision, trace.CurrentSpan.SamplingStatus);
            trace.ForceSampled();
            Assert.AreEqual(SamplingStatus.Sampled, trace.CurrentSpan.SamplingStatus);
        }

    }
}
