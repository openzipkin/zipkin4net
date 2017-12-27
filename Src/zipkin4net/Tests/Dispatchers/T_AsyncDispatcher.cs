using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using zipkin4net.Dispatcher;
using zipkin4net.Logger;
using zipkin4net.Utils;
using Moq;
using NUnit.Framework;

namespace zipkin4net.UTest.Dispatchers
{
    [TestFixture(true)]
    [TestFixture(false)]
    internal class T_AsyncDispatcher
    {
        private readonly bool _useConcurrentQueueDispatcher;

        public T_AsyncDispatcher(bool useConcurrentQueueDispatcher)
        {
            _useConcurrentQueueDispatcher = useConcurrentQueueDispatcher;
        }

        [Test]
        public void RecordShouldBeDispatched()
        {
            var sync = new ManualResetEvent(false);

            var record = new Record(new SpanState(1, 0, 1, isSampled: null, isDebug: false), TimeUtils.UtcNow, Annotations.ClientRecv());

            Record dispatchedRecord = null;

            var dispatcher = GetRecordDispatcher(r =>
            {
                dispatchedRecord = r;
                sync.Set();
            }, new VoidLogger());

            dispatcher.Dispatch(record);
            sync.WaitOne();

            dispatcher.Stop();

            Assert.AreEqual(record, dispatchedRecord);
        }

        [Test]
        public void RecordShouldnotBeDispatchedIfStopped()
        {
            var sync = new ManualResetEvent(false);

            var record = new Record(new SpanState(1, 0, 1, isSampled: null, isDebug: false), TimeUtils.UtcNow, Annotations.ClientRecv());

            int recordsDispatched = 0;

            var dispatcher = GetRecordDispatcher(r =>
            {
                Interlocked.Increment(ref recordsDispatched);
                sync.Set();
            }, new VoidLogger());

            dispatcher.Dispatch(record);
            sync.WaitOne();

            dispatcher.Stop();

            dispatcher.Dispatch(record);

            // wait to see if the message eventually gets dispatched
            Thread.Sleep(500);

            Assert.AreEqual(1, recordsDispatched);
        }

        [Test]
        public void RecordsShouldBeDispatchedInOrder()
        {
            var sync = new CountdownEvent(2);
            var traceId = 1;
            var firstRecord = new Record(new SpanState(traceId, 0, 1, isSampled: null, isDebug: false), TimeUtils.UtcNow, Annotations.ClientRecv());
            var secondRecord = new Record(new SpanState(traceId, 0, 1, isSampled: null, isDebug: false), TimeUtils.UtcNow, Annotations.ClientRecv());

            var queue = new ConcurrentQueue<Record>();

            var dispatcher = GetRecordDispatcher(r =>
            {
                queue.Enqueue(r);
                sync.Signal();
            }, new VoidLogger());

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

        [Test]
        [Ignore("Flaky on loaded jenkins slaves")]
        public void DispatcherShouldNotEnqueueMessagesInfinitely()
        {
            var record = new Record(new SpanState(1, 0, 1, isSampled: null, isDebug: false), TimeUtils.UtcNow, Annotations.ClientRecv());
            var logger = new Mock<ILogger>();

            const int maxCapacity = 10;

            var dispatcher = GetRecordDispatcher(r =>
            {
                Thread.Sleep(TimeSpan.FromDays(1)); // long running operation
            }, logger.Object, maxCapacity);

            bool dispatchSuccess = true;

            var task = Task.Factory.StartNew(() =>
            {
                for (var i = 0; i < maxCapacity; ++i)
                {
                    dispatcher.Dispatch(record);
                }
                dispatchSuccess = dispatcher.Dispatch(record); // maxCapacity + 1
            });

            task.Wait();
            dispatcher.Stop();

            Assert.IsFalse(dispatchSuccess);
        }

        public IRecordDispatcher GetRecordDispatcher(Action<Record> pushToTracers, ILogger logger, int maxCapacity = 5000)
        {
            if (_useConcurrentQueueDispatcher)
            {
                return new InOrderAsyncQueueDispatcher(pushToTracers, maxCapacity);
            }
            return new InOrderAsyncActionBlockDispatcher(pushToTracers, maxCapacity);
        }
    }

}
