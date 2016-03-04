using System;
using System.Collections.Concurrent;
using System.Threading;
using Criteo.Profiling.Tracing.Dispatcher;
using Criteo.Profiling.Tracing.Utils;
using NUnit.Framework;

namespace Criteo.Profiling.Tracing.UTest.Dispatchers
{
    [TestFixture]
    class T_AsyncDispatcher
    {

        [Test]
        public void RecordShouldBeDispatched()
        {
            var sync = new AutoResetEvent(false);

            var record = new Record(new SpanState(1, 0, 1, Flags.Empty), TimeUtils.UtcNow, Annotations.ClientRecv());

            var dispatcher = new InOrderAsyncDispatcher(r =>
            {
                Assert.AreEqual(record, r);
                sync.Set();
            });

            dispatcher.Dispatch(record);
            sync.WaitOne();

            dispatcher.Stop();
        }

        [Test]
        public void RecordsShouldBeDispatchedInOrder()
        {
            var sync = new CountdownEvent(2);

            var firstRecord = new Record(new SpanState(1, 0, 1, Flags.Empty), TimeUtils.UtcNow, Annotations.ClientRecv());
            var secondRecord = new Record(new SpanState(1, 0, 1, Flags.Empty), TimeUtils.UtcNow, Annotations.ClientRecv());


            var queue = new ConcurrentQueue<Record>();

            var dispatcher = new InOrderAsyncDispatcher(r =>
            {
                queue.Enqueue(r);
                sync.Signal();
            });

            dispatcher.Dispatch(firstRecord);
            dispatcher.Dispatch(secondRecord);
            sync.Wait();

            Assert.AreEqual(2, queue.Count);

            Record record;

            Assert.IsTrue(queue.TryDequeue(out record));
            Assert.AreEqual(firstRecord, record);

            Assert.IsTrue(queue.TryDequeue(out record));
            Assert.AreEqual(secondRecord, record);

            dispatcher.Stop();
        }
    }

}
