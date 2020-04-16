using NUnit.Framework;
using Moq;
using zipkin4net.Dispatcher;
using zipkin4net.Logger;
using zipkin4net.Annotation;

namespace zipkin4net.UTest
{
    [TestFixture]
    internal class T_ServerTrace
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
        public void ShouldLogServerAnnotations()
        {
            // Arrange
            dispatcher
                .Setup(h => h.Dispatch(It.IsAny<Record>()))
                .Returns(true);

            // Act
            var trace = Trace.Create();
            trace.ForceSampled();
            Trace.Current = trace;
            using (var server = new ServerTrace(serviceName, rpc))
            {
                // Assert
                dispatcher
                    .Verify(h =>
                        h.Dispatch(It.Is<Record>(m =>
                            m.Annotation is ServerRecv)));

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
                        m.Annotation is ServerSend)));
        }
    }
}