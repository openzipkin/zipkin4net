using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Criteo.Profiling.Tracing.UTest
{
    [TestFixture]
    class T_TraceContext
    {
        [Test]
        public void ContextCanBeSetThenGetAndCleared()
        {
            var trace = Trace.Create();
            TraceContext.Set(trace);
            Assert.AreEqual(trace, TraceContext.Get());

            TraceContext.Clear();
            Assert.IsNull(TraceContext.Get());
        }

        [Test]
        public void SetOverridesPreviousTrace()
        {
            var firstTrace = Trace.Create();
            var secondTrace = Trace.Create();

            TraceContext.Set(firstTrace);
            TraceContext.Set(secondTrace);

            Assert.AreEqual(secondTrace, TraceContext.Get());
        }

        [Test]
        public void ContextIsPassedToCreatedThreads()
        {
            var trace = Trace.Create();
            TraceContext.Set(trace);

            Trace threadTrace = null;
            Trace threadTrace2 = null;

            var thread = new Thread(() =>
            {
                threadTrace = TraceContext.Get();
            });
            thread.Start();

            TraceContext.Clear();

            var thread2 = new Thread(() =>
            {
                threadTrace2 = TraceContext.Get();
            });
            thread2.Start();

            thread.Join();
            thread2.Join();

            Assert.AreEqual(trace, threadTrace);
            Assert.IsNull(threadTrace2);
        }

        [Test]
        public void ContextCannotBeClearedInContinuation()
        {
            var trace = Trace.Create();
            TraceContext.Set(trace);

            Trace taskTrace = null;
            Trace taskTraceContinuation = null;

            var task = Task.Run(async () =>
            {
                await Task.Yield();

                taskTrace = TraceContext.Get();

                TraceContext.Clear(); // does not impact the continuation
            }).ContinueWith(async previousTask =>
            {
                await Task.Yield();

                taskTraceContinuation = TraceContext.Get();
            });

            task.Wait();

            Assert.AreEqual(trace, taskTrace);
            Assert.AreEqual(trace, taskTraceContinuation);
            Assert.AreEqual(trace, TraceContext.Get()); // Current context still contains the trace despite the clear in the task
        }
    }
}
