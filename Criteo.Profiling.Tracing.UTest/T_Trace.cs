using Moq;
using NUnit.Framework;

namespace Criteo.Profiling.Tracing.UTest
{
    [TestFixture]
    class T_Trace
    {

        private Mock<ITracer> _mockTracer;

        [SetUp]
        public void SetUp()
        {
            TraceManager.ClearTracers();
            TraceManager.Start(new Configuration());
            TraceManager.SamplingRate = 1f;

            _mockTracer = new Mock<ITracer>();
            TraceManager.RegisterTracer(_mockTracer.Object);
        }

        [TearDown]
        public void TearDown()
        {
            TraceManager.Stop();
            TraceManager.ClearTracers();
        }

        [Test]
        public void TracerRecordShouldBeCalledIfTracingIsStarted()
        {
            var trace = Trace.CreateIfSampled();

            Assert.IsTrue(TraceManager.Started);
            trace.Record(Annotations.ServerSend());
            TraceManager.Stop();

            _mockTracer.Verify(tracer => tracer.Record(It.IsAny<Record>()), Times.Once());
        }

        [Test]
        public void RecordsShouldntBeSentToTracersIfTracingIsStopped()
        {
            var trace = Trace.CreateIfSampled();

            TraceManager.Stop();
            Assert.IsFalse(TraceManager.Started);
            trace.Record(Annotations.ServerSend());

            _mockTracer.Verify(tracer => tracer.Record(It.IsAny<Record>()), Times.Never());
        }

        [Test]
        public void ChildTraceIsCorrectlyCreated()
        {
            var parent = Trace.CreateIfSampled();
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
            var trace = Trace.CreateIfSampled();

            var clientRcv = Annotations.ClientRecv();
            trace.Record(clientRcv);

            TraceManager.Stop();

            _mockTracer.Verify(t => t.Record(It.Is<Record>(r => r.Annotation == clientRcv && r.SpanId.Equals(trace.CurrentId))), Times.Once());
        }

        [Test]
        public void CannotStartMultipleTimes()
        {
            Assert.True(TraceManager.Started, "Test setup failed?");
            Assert.False(TraceManager.Start(new Configuration()));
        }

        [Test]
        public void TraceSamplingForced()
        {
            var trace = Trace.CreateIfSampled();

            Assert.False(trace.CurrentId.Flags.IsSamplingKnown());
            Assert.False(trace.CurrentId.Flags.IsSampled());
            trace.ForceSampled();
            Assert.True(trace.CurrentId.Flags.IsSamplingKnown());
            Assert.True(trace.CurrentId.Flags.IsSampled());
        }
    }
}
