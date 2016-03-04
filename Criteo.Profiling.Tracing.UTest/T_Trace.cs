using Criteo.Profiling.Tracing.Dispatcher;
using Moq;
using NUnit.Framework;

namespace Criteo.Profiling.Tracing.UTest
{
    [TestFixture]
    class T_Trace
    {

        private readonly SpanId _spanId = new SpanId(1, null, 2, Flags.Empty);

        [SetUp]
        public void SetUp()
        {
            TraceManager.ClearTracers();
            TraceManager.Stop();

        }

        [Test]
        public void TracerRecordShouldBeCalledIfTracingIsStarted()
        {
            var trace = Trace.CreateFromId(_spanId);

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
            var trace = Trace.CreateFromId(_spanId);

            var mockTracer = new Mock<ITracer>();
            TraceManager.RegisterTracer(mockTracer.Object);

            Assert.IsFalse(TraceManager.Started);

            trace.Record(Annotations.ServerSend());

            mockTracer.Verify(tracer => tracer.Record(It.IsAny<Record>()), Times.Never());
        }

        [Test]
        public void ChildTraceIsCorrectlyCreated()
        {
            var parent = Trace.CreateFromId(_spanId);
            var child = parent.Child();

            // Should share the same global id
            Assert.AreEqual(parent.CurrentId.TraceId, child.CurrentId.TraceId);
            Assert.AreEqual(parent.CorrelationId, child.CorrelationId);

            // Parent id of the child should be the parent span id
            Assert.AreEqual(parent.CurrentId.Id, child.CurrentId.ParentSpanId);

            // Flags should be copied
            Assert.AreEqual(parent.CurrentId.Flags, child.CurrentId.Flags);

            // Child cannot have the same span id
            Assert.AreNotEqual(parent.CurrentId.Id, child.CurrentId.Id);
        }

        [Test]
        public void TraceCreatesCorrectRecord()
        {
            var trace = Trace.CreateFromId(_spanId);

            var dispatcher = new Mock<IRecordDispatcher>();
            TraceManager.Start(new Configuration(), dispatcher.Object);

            var clientRcv = Annotations.ClientRecv();
            trace.Record(clientRcv);

            dispatcher.Verify(d => d.Dispatch(It.Is<Record>(r => r.Annotation == clientRcv && r.SpanId.Equals(trace.CurrentId))), Times.Once());
        }


        [Test]
        public void TraceSamplingForced()
        {
            var trace = Trace.CreateFromId(_spanId);

            Assert.False(trace.CurrentId.Flags.IsSamplingKnown());
            Assert.False(trace.CurrentId.Flags.IsSampled());
            trace.ForceSampled();
            Assert.True(trace.CurrentId.Flags.IsSamplingKnown());
            Assert.True(trace.CurrentId.Flags.IsSampled());
        }

    }
}
