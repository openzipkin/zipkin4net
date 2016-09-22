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
        private const int WaitSleepTimeMs = 500;

        private readonly int _timeoutOnStopMs;
        private readonly BlockingCollection<Record> _queue;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public InOrderAsyncQueueDispatcher(Action<Record> pushToTracers, int maxCapacity = 5000, int timeoutOnStopMs = 10000)
        {
            _timeoutOnStopMs = timeoutOnStopMs;
            _queue = new BlockingCollection<Record>(maxCapacity);
            _cancellationTokenSource = new CancellationTokenSource();

            Task.Factory.StartNew(() =>
            {
                while (!_queue.IsCompleted && !_cancellationTokenSource.IsCancellationRequested)
                {
                    var record = _queue.Take(_cancellationTokenSource.Token);
                    pushToTracers(record);
                }
            }, _cancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        public void Stop()
        {
            _queue.CompleteAdding();

            int maxWaitLoop = _timeoutOnStopMs / WaitSleepTimeMs;

            while (maxWaitLoop > 0 && !_queue.IsCompleted)
            {
                Thread.Sleep(WaitSleepTimeMs);
                maxWaitLoop--;
            }

            _cancellationTokenSource.Cancel(throwOnFirstException: false);
        }

        public bool Dispatch(Record record)
        {
            if (!_queue.IsAddingCompleted)
            {
                return _queue.TryAdd(record);
            }
            return false;
        }
    }
}
