using NUnit.Framework;
using System.Threading.Tasks;
using System;
using Moq;
using zipkin4net.Dispatcher;
using zipkin4net.Logger;
using zipkin4net.Annotation;

namespace zipkin4net.UTest
{
    [TestFixture]
    internal class T_BaseStandardTrace
    {
        private Mock<IRecordDispatcher> dispatcher;

        [SetUp]
        public void SetUp()
        {
            TraceManager.ClearTracers();
            TraceManager.Stop();
            dispatcher = new Mock<IRecordDispatcher>();
            TraceManager.Start(new VoidLogger(), dispatcher.Object);
        }

        [Test]
        public void ShouldLogAnnotation()
        {
            // Arrange
            dispatcher
                .Setup(h => h.Dispatch(It.IsAny<Record>()))
                .Returns(true);

            var trace = Trace.Create();
            trace.ForceSampled();
            Trace.Current = trace;
            var baseStandardTrace = new BaseStandardTrace
            {
                Trace = trace
            };

            // Act
            baseStandardTrace.AddAnnotation(Annotations.WireSend());

            // Assert
            dispatcher
                .Verify(h =>
                    h.Dispatch(It.Is<Record>(m =>
                        m.Annotation is WireSend)));
        }

        [Test]
        public void ExceptionThrownInTracedActionAsyncTShouldAddErrorTagAndRethrow()
        {
            var trace = Trace.Create();
            trace.ForceSampled();
            Trace.Current = trace;
            var baseStandardTrace = new BaseStandardTrace
            {
                Trace = trace
            };
            var ex = new Exception("something bad happened");
            Task<int> task = Task.Run(() =>
            {
                try
                {
                    return 0;
                }
                finally
                {
                    throw ex;
                }
            });
            Assert.ThrowsAsync<Exception>(() => baseStandardTrace.TracedActionAsync(task));

            VerifyDispatcherRecordedAnnotation(new TagAnnotation("error", ex.Message));
        }

        [Test]
        public void ExceptionThrownInTracedActionAsyncTShouldBeRethrownWhenCurrentTraceIsNull()
        {
            Trace.Current = null;
            var baseStandardTrace = new BaseStandardTrace();

            Task<object> task = Task.Run<object>(() => throw new SomeException());

            Assert.ThrowsAsync<SomeException>(() => baseStandardTrace.TracedActionAsync(task));
        }

        [Test]
        public void ExceptionThrownInTracedActionAsyncShouldAddErrorTagAndRethrow()
        {
            var trace = Trace.Create();
            trace.ForceSampled();
            Trace.Current = trace;
            var baseStandardTrace = new BaseStandardTrace
            {
                Trace = trace
            };
            var ex = new Exception("something bad happened");
            Task task = Task.Run(() =>
            {
                try
                {
                    return;
                }
                finally
                {
                    throw ex;
                }
            });
            Assert.ThrowsAsync<Exception>(() => baseStandardTrace.TracedActionAsync(task));

            VerifyDispatcherRecordedAnnotation(new TagAnnotation("error", ex.Message));
        }

        [Test]
        public void ExceptionThrownInTracedActionAsyncShouldBeRethrownWhenCurrentTraceIsNull()
        {
            Trace.Current = null;
            var baseStandardTrace = new BaseStandardTrace();

            Task task = Task.Run(() => throw new SomeException());

            Assert.ThrowsAsync<SomeException>(() => baseStandardTrace.TracedActionAsync(task));
        }

        private void VerifyDispatcherRecordedAnnotation(IAnnotation annotation)
        {
            dispatcher.Verify(d => d.Dispatch(It.Is<Record>(r => r.Annotation.Equals(annotation))));
        }

        private class SomeException : Exception
        {
        }
    }
}