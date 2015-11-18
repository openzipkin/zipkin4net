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
            Tracer.Clear();
            Trace.TracingEnabled = true;
            Trace.SamplingRate = 1f;
        }

        [Test]
        public void TracerRecordShouldBeCalledIfTracingIsEnabled()
        {
            var mockTracer = new Mock<ITracer>();
            Tracer.Register(mockTracer.Object);

            Trace.TracingEnabled = true;
            var trace = Trace.CreateIfSampled();
            trace.Record(Annotations.ServerSend());

            Trace.TracingEnabled = false; // force flush to tracers

            mockTracer.Verify(tracer => tracer.Record(It.IsAny<Record>()), Times.Once());
        }

        [Test]
        public void TracerRecordShouldntBeCalledIfTracingIsDisabled()
        {
            var mockTracer = new Mock<ITracer>();
            Tracer.Register(mockTracer.Object);

            Trace.TracingEnabled = false;
            var trace = Trace.CreateIfSampled();
            trace.Record(Annotations.ServerSend());

            Trace.TracingEnabled = false; // force flush to tracers

            mockTracer.Verify(tracer => tracer.Record(It.IsAny<Record>()), Times.Never());
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
            var mockTracer = new Mock<ITracer>();
            Tracer.Register(mockTracer.Object);

            var trace = Trace.CreateIfSampled();

            var clientRcv = Annotations.ClientRecv();

            trace.Record(clientRcv);

            Trace.TracingEnabled = false; // force flush to tracers

            mockTracer.Verify(t => t.Record(It.Is<Record>(r => r.Annotation == clientRcv && r.SpanId.Equals(trace.CurrentId))), Times.Once());
        }

    }
}
