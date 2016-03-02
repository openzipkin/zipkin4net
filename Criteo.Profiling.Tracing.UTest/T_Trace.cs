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
            Tracer.Clear();
            Trace.Start(new Configuration());
            Trace.SamplingRate = 1f;

            _mockTracer = new Mock<ITracer>();
            Tracer.Register(_mockTracer.Object);
        }

        [TearDown]
        public void TearDown()
        {
            Trace.Stop();
            Tracer.Clear();
        }

        [Test]
        public void TracerRecordShouldBeCalledIfTracingIsStarted()
        {
            var trace = Trace.CreateIfSampled();

            Assert.IsTrue(Trace.TracingRunning);
            trace.Record(Annotations.ServerSend());
            Trace.Stop();

            _mockTracer.Verify(tracer => tracer.Record(It.IsAny<Record>()), Times.Once());
        }

        [Test]
        public void RecordsShouldntBeSentToTracersIfTracingIsStopped()
        {
            var trace = Trace.CreateIfSampled();

            Trace.Stop();
            Assert.IsFalse(Trace.TracingRunning);
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

            Trace.Stop();

            _mockTracer.Verify(t => t.Record(It.Is<Record>(r => r.Annotation == clientRcv && r.SpanId.Equals(trace.CurrentId))), Times.Once());
        }

        [Test]
        public void CannotStartMultipleTimes()
        {
            Assert.True(Trace.TracingRunning, "Test setup failed?");
            Assert.False(Trace.Start(new Configuration()));
        }

    }
}
