using System;
using System.IO;
#if !NETCOREAPP1_0
using System.Runtime.Serialization.Formatters.Binary;
#endif
using Criteo.Profiling.Tracing.Dispatcher;
using Criteo.Profiling.Tracing.Logger;
using Criteo.Profiling.Tracing.Utils;
using Moq;
using NUnit.Framework;

namespace Criteo.Profiling.Tracing.UTest
{
    [TestFixture]
    internal class T_Trace
    {
        [SetUp]
        public void SetUp()
        {
            TraceManager.ClearTracers();
            TraceManager.Stop();
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
            Assert.AreEqual(parentTrace.CurrentSpan.Flags, childTrace.CurrentSpan.Flags);

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


        [TestCase(SpanFlags.SamplingKnown)]
        [TestCase(SpanFlags.None)]
        [TestCase(SpanFlags.SamplingKnown | SpanFlags.Sampled)]
        public void TraceSamplingForced(SpanFlags initialFlags)
        {
            var spanState = new SpanState(1, 0, 1, initialFlags);
            var trace = Trace.CreateFromId(spanState);
            trace.ForceSampled();

            Assert.AreEqual(SamplingStatus.Sampled, trace.CurrentSpan.SamplingStatus);
        }

        [Test]
        public void FlagSampledShouldForward()
        {
            var dispatcher = new Mock<IRecordDispatcher>();
            TraceManager.Start(new VoidLogger(), dispatcher.Object);

            var spanState = new SpanState(1, 0, 1, SpanFlags.SamplingKnown | SpanFlags.Sampled);
            var trace = Trace.CreateFromId(spanState);

            trace.Record(Annotations.ClientRecv());

            dispatcher.Verify(d => d.Dispatch(It.IsAny<Record>()), Times.Once());
        }

        [Test]
        public void FlagNotSampledShouldNotForward()
        {
            var dispatcher = new Mock<IRecordDispatcher>();
            TraceManager.Start(new VoidLogger(), dispatcher.Object);

            var spanState = new SpanState(1, 0, 1, SpanFlags.SamplingKnown);
            var trace = Trace.CreateFromId(spanState);

            trace.Record(Annotations.ClientRecv());

            dispatcher.Verify(d => d.Dispatch(It.IsAny<Record>()), Times.Never);
        }

        [Test]
        public void FlagUnsetShouldForwardForBackwardCompatibility()
        {
            var dispatcher = new Mock<IRecordDispatcher>();
            TraceManager.Start(new VoidLogger(), dispatcher.Object);

            var spanState = new SpanState(1, 0, 1, SpanFlags.None);
            var trace = Trace.CreateFromId(spanState);

            trace.Record(Annotations.ClientRecv());

            dispatcher.Verify(d => d.Dispatch(It.IsAny<Record>()), Times.Once());
        }

    }
}
