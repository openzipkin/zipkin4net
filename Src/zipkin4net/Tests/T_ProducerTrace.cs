using NUnit.Framework;
using Moq;
using zipkin4net.Dispatcher;
using zipkin4net.Logger;
using zipkin4net.Annotation;

namespace zipkin4net.UTest
{
    [TestFixture]
    internal class T_ProducerTrace
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
        public void ShouldNotSetCurrentTrace()
        {
            Trace.Current = null;
            using (var producer = new ProducerTrace(serviceName, rpc))
            {
                Assert.IsNull(producer.Trace);
            }
        }

        [Test]
        public void ShouldCallChildWhenCurrentTraceNotNull()
        {
            var trace = Trace.Create();
            Trace.Current = trace;
            using (var producer = new ProducerTrace(serviceName, rpc))
            {
                Assert.AreEqual(trace.CurrentSpan.SpanId, producer.Trace.CurrentSpan.ParentSpanId);
                Assert.AreEqual(trace.CurrentSpan.TraceId, producer.Trace.CurrentSpan.TraceId);
            }
        }

        [Test]
        public void ShouldLogProducerAnnotations()
        {
            // Arrange
            dispatcher
                .Setup(h => h.Dispatch(It.IsAny<Record>()))
                .Returns(true);

            // Act
            var trace = Trace.Create();
            trace.ForceSampled();
            Trace.Current = trace;
            using (var producer = new ProducerTrace(serviceName, rpc))
            {
                // Assert
                dispatcher
                    .Verify(h =>
                        h.Dispatch(It.Is<Record>(m =>
                            m.Annotation is ProducerStart)));

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
                        m.Annotation is ProducerStop)));
        }
    }
}