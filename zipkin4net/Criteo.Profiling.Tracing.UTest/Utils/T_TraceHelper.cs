using System;
using Criteo.Profiling.Tracing.Annotation;
using Criteo.Profiling.Tracing.Dispatcher;
using Criteo.Profiling.Tracing.Logger;
using Criteo.Profiling.Tracing.Utils;
using NUnit.Framework;
using Moq;
using System.Threading.Tasks;

namespace Criteo.Profiling.Tracing.UTest.Utils
{

    [TestFixture]
    internal class T_TraceHelper
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
        public void ExceptionThrownInTracedActionShouldAddErrorTagAndRethrow()
        {
            Trace.Current = CreateSampledTrace();
            var ex = new Exception("something bad happened");
            Assert.Throws<Exception>(() => TraceHelper.TracedAction(() => { throw ex; }));
            VerifyDispatcherRecordedAnnotation(new TagAnnotation("error", ex.Message));
        }

        [Test]
        public void ExceptionThrownInTracedActionAsyncShouldAddErrorTagAndRethrow()
        {
            Trace.Current = CreateSampledTrace();
            var ex = new Exception("something bad happened");
            Assert.ThrowsAsync<Exception>(() => TraceHelper.TracedActionAsync(Task.Run(() => { throw ex; })));
            
            VerifyDispatcherRecordedAnnotation(new TagAnnotation("error", ex.Message));
        }

        [Test]
        public void ExceptionThrownInTracedActionAsyncTypedShouldAddErrorTagAndRethrow()
        {
            Trace.Current = CreateSampledTrace();
            var ex = new Exception("something bad happened");
            Task<int> task = Task.Run(() =>
            {
                try {
                    return 0; 
                } finally {
                    throw ex;
                }
            });
            Assert.ThrowsAsync<Exception>(() => TraceHelper.TracedActionAsync(task));
            
            VerifyDispatcherRecordedAnnotation(new TagAnnotation("error", ex.Message));
        }

        private static Trace CreateSampledTrace()
        {
            var trace = Trace.Create();
            trace.ForceSampled();
            return trace;
        }

        private void VerifyDispatcherRecordedAnnotation(IAnnotation annotation)
        {
            dispatcher.Verify(d => d.Dispatch(It.Is<Record>(r => r.Annotation.Equals(annotation))));
        }
    }
}