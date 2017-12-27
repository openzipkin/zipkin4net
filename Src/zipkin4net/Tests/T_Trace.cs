using System;
#if !NET_CORE
using System.Runtime.Serialization.Formatters.Binary;
#endif
using zipkin4net.Dispatcher;
using zipkin4net.Logger;
using Moq;
using NUnit.Framework;

namespace zipkin4net.UTest
{
    [TestFixture]
    internal class T_Trace
    {
        [SetUp]
        public void SetUp()
        {
            TraceManager.ClearTracers();
            TraceManager.Stop();
            TraceManager.Trace128Bits = false;
        }

        [Test]
        public void ChildTraceIsCorrectlyCreated()
        {
            var parentTrace = Trace.Create();
            var childTrace = parentTrace.Child();

            // Should share the same global id
            Assert.AreEqual(parentTrace.CurrentSpan.TraceId, childTrace.CurrentSpan.TraceId);
            Assert.AreEqual(parentTrace.CorrelationId, childTrace.CorrelationId);

            // Parent id of the child should be the parent span id
            Assert.AreEqual(parentTrace.CurrentSpan.SpanId, childTrace.CurrentSpan.ParentSpanId);

            // Flags should be copied
            Assert.AreEqual(parentTrace.CurrentSpan.Sampled, childTrace.CurrentSpan.Sampled);
            Assert.AreEqual(parentTrace.CurrentSpan.Debug, childTrace.CurrentSpan.Debug);

            // Child cannot have the same span id
            Assert.AreNotEqual(parentTrace.CurrentSpan.SpanId, childTrace.CurrentSpan.SpanId);
        }

        [Test]
        public void TraceCreatesCorrectRecord()
        {
            var trace = Trace.Create();
            trace.ForceSampled();

            var dispatcher = new Mock<IRecordDispatcher>();
            TraceManager.Start(new VoidLogger(), dispatcher.Object);

            var someAnnotation = Annotations.ClientRecv();
            trace.Record(someAnnotation);

            dispatcher.Verify(d => d.Dispatch(It.Is<Record>(r => r.Annotation.Equals(someAnnotation) && r.SpanState.Equals(trace.CurrentSpan))), Times.Once());
        }

        [Test]
        public void TraceCreatesCorrectRecordWithTimeSpecified()
        {
            var trace = Trace.Create();
            trace.ForceSampled();

            var dispatcher = new Mock<IRecordDispatcher>();
            TraceManager.Start(new VoidLogger(), dispatcher.Object);

            var someAnnotation = Annotations.ClientRecv();
            var recordTime = new DateTime(2010, 2, 3, 14, 3, 1, DateTimeKind.Utc);
            trace.Record(someAnnotation, recordTime);

            dispatcher.Verify(d => d.Dispatch(It.Is<Record>(r => r.Annotation.Equals(someAnnotation) && r.SpanState.Equals(trace.CurrentSpan) && r.Timestamp.Equals(recordTime))), Times.Once());
        }


        [TestCase(false)]
        [TestCase(null)]
        [TestCase(true)]
        public void TraceSamplingForced(bool? isSampled)
        {
            var spanState = new SpanState(1, 0, 1, isSampled: isSampled, isDebug: false);
            var trace = Trace.CreateFromId(spanState);
            trace.ForceSampled();

            Assert.IsTrue(trace.CurrentSpan.Sampled);
        }

        [Test]
        public void FlagSampledShouldForward()
        {
            var dispatcher = new Mock<IRecordDispatcher>();
            TraceManager.Start(new VoidLogger(), dispatcher.Object);

            var spanState = new SpanState(1, 0, 1, isSampled: true, isDebug: false);
            var trace = Trace.CreateFromId(spanState);

            trace.Record(Annotations.ClientRecv());

            dispatcher.Verify(d => d.Dispatch(It.IsAny<Record>()), Times.Once());
        }

        [Test]
        public void FlagNotSampledShouldNotForward()
        {
            var dispatcher = new Mock<IRecordDispatcher>();
            TraceManager.Start(new VoidLogger(), dispatcher.Object);

            var spanState = new SpanState(1, 0, 1, isSampled: false, isDebug: false);
            var trace = Trace.CreateFromId(spanState);

            trace.Record(Annotations.ClientRecv());

            dispatcher.Verify(d => d.Dispatch(It.IsAny<Record>()), Times.Never);
        }

        [Test]
        public void FlagUnsetShouldNotForward()
        {
            var dispatcher = new Mock<IRecordDispatcher>();
            TraceManager.Start(new VoidLogger(), dispatcher.Object);

            var spanState = new SpanState(1, 0, 1, isSampled: null, isDebug: false);
            var trace = Trace.CreateFromId(spanState);

            trace.Record(Annotations.ClientRecv());

            dispatcher.Verify(d => d.Dispatch(It.IsAny<Record>()), Times.Never());
        }

        [Test]
        public void TraceIdHighIsGeneratedIf128BitsActivated()
        {
            TraceManager.Trace128Bits = true;
            var trace = Trace.Create();
            Assert.AreNotEqual(SpanState.NoTraceIdHigh, trace.CurrentSpan.TraceIdHigh);
        }

        [Test]
        public void TraceIdHighIsNotGeneratedIf128BitsDeactivated()
        {
            TraceManager.Trace128Bits = false;
            var trace = Trace.Create();
            Assert.AreEqual(SpanState.NoTraceIdHigh, trace.CurrentSpan.TraceIdHigh);
        }
    }
}
