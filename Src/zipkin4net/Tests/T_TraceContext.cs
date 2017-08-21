using System.Threading;
using NUnit.Framework;

namespace zipkin4net.UTest
{
    [TestFixture]
    internal class T_TraceContext
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
    }
}
