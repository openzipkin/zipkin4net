using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Criteo.Profiling.Tracing.Dispatcher
{
    /// <summary>
    /// Dispatch messages asynchronously in order using a concurrent queue and a consumer task
    /// </summary>
    internal class InOrderAsyncQueueDispatcher : IRecordDispatcher
    {
        private readonly BlockingCollection<Record> _queue;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public InOrderAsyncQueueDispatcher(Action<Record> pushToTracers, int maxCapacity = 5000)
        {
            _queue = new BlockingCollection<Record>(maxCapacity);
            _cancellationTokenSource = new CancellationTokenSource();

            Task.Factory.StartNew(() =>
            {
                while (!_cancellationTokenSource.IsCancellationRequested)
                {
                    var record = _queue.Take(_cancellationTokenSource.Token);
                    pushToTracers(record);
                }
            }, _cancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        public void Stop()
        {
            _cancellationTokenSource.Cancel(throwOnFirstException: false);
        }

        public bool Dispatch(Record record)
        {
            return _queue.TryAdd(record);
        }
    }
}
