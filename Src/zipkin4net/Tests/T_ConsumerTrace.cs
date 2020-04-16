using NUnit.Framework;
using Moq;
using zipkin4net.Dispatcher;
using zipkin4net.Logger;
using zipkin4net.Annotation;
using zipkin4net.Propagation;

namespace zipkin4net.UTest
{
    [TestFixture]
    internal class T_ConsumerTrace
    {
        private Mock<IRecordDispatcher> dispatcher;
        private const string serviceName = "service1";
        private const string rpc = "rpc";

        [SetUp]
        public void SetUp()
        {
            TraceManager.ClearTracers();
            TraceManager.Stop();
            dispatcher = new Mock<IRecordDispatcher>();
            TraceManager.Start(new VoidLogger(), dispatcher.Object);
        }

        [Test]
        public void ShouldSetCurrentTraceIfInvalidTraceInformationIsPassed()
        {
            TraceManager.SamplingRate = 1.0f;
            using (var client = new ConsumerTrace(serviceName, rpc, null, null, null, null, null))
            {
                Assert.IsNotNull(client.Trace);
            }
        }

        [Test]
        public void ShouldSetChildTraceIfValidTraceInformationIsPassed()
        {
            TraceManager.SamplingRate = 1.0f;
            var rootTrace = Trace.Create();
            var trace = rootTrace.Child();
            var context = trace.CurrentSpan;
            using (var client = new ConsumerTrace(serviceName, rpc,
                context.SerializeTraceId(),
                context.SerializeSpanId(),
                context.SerializeParentSpanId(),
                context.SerializeSampledKey(),
                context.SerializeDebugKey()))
            {
                Assert.AreEqual(trace.CurrentSpan.SpanId, client.Trace.CurrentSpan.ParentSpanId);
                Assert.AreEqual(trace.CurrentSpan.TraceId, client.Trace.CurrentSpan.TraceId);
            }
        }

        [Test]
        public void ShouldLogConsumerAnnotations()
        {
            // Arrange
            dispatcher
                .Setup(h => h.Dispatch(It.IsAny<Record>()))
                .Returns(true);

            // Act
            TraceManager.SamplingRate = 1.0f;
            using (var server = new ConsumerTrace(serviceName, rpc, null, null, null, null, null))
            {
                // Assert
                dispatcher
                    .Verify(h =>
                        h.Dispatch(It.Is<Record>(m =>
                            m.Annotation is ConsumerStart)));

                dispatcher
                    .Verify(h =>
                        h.Dispatch(It.Is<Record>(m =>
                            m.Annotation is ServiceName
                            && ((ServiceName)m.Annotation).Service == serviceName)));

                dispatcher
                    .Verify(h =>
                        h.Dispatch(It.Is<Record>(m =>
                            m.Annotation is Rpc
                            && ((Rpc)m.Annotation).Name == rpc)));
            }

            // Assert
            dispatcher
                .Verify(h =>
                    h.Dispatch(It.Is<Record>(m =>
                        m.Annotation is ConsumerStop)));
        }
    }
}