using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace zipkin4net.Dispatcher
{
    /// <summary>
    /// Dispatch messages asynchronously in order using a concurrent queue and a consumer task
    /// </summary>
    internal class InOrderAsyncQueueDispatcher : IRecordDispatcher
    {
        private readonly Action<Record> _pushToTracers;
        private readonly int _maxCapacity;
        private readonly int _timeoutOnStopMs;
        private readonly ConcurrentQueue<Record> _queue;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly ManualResetEventSlim _eventSlim;
        private readonly Task _processQueueTask;

        public InOrderAsyncQueueDispatcher(Action<Record> pushToTracers, int maxCapacity = 5000, int timeoutOnStopMs = 10000)
        {
            _pushToTracers = pushToTracers;
            _maxCapacity = maxCapacity;
            _timeoutOnStopMs = timeoutOnStopMs;
            _queue = new ConcurrentQueue<Record>();
            _cancellationTokenSource = new CancellationTokenSource();
            _eventSlim = new ManualResetEventSlim(false, spinCount: 1);

            _processQueueTask = Task.Factory.StartNew(Consumer, _cancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        public void Stop()
        {
            _cancellationTokenSource.Cancel(throwOnFirstException: false);
            _processQueueTask.Wait(_timeoutOnStopMs);
        }

        public bool Dispatch(Record record)
        {
            if (ShouldEnqueueRecord())
            {
                _queue.Enqueue(record);
                if (!_eventSlim.IsSet)
                {
                    _eventSlim.Set();
                }
                return true;
            }

            return false;
        }

        private bool ShouldEnqueueRecord()
        {
            return !_cancellationTokenSource.IsCancellationRequested && _queue.Count < _maxCapacity;
        }

        private void Consumer()
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                ProcessQueue();

                try
                {
                    _eventSlim.Wait(_cancellationTokenSource.Token);
                    _eventSlim.Reset();
                }
                catch (OperationCanceledException)
                {
                    // expected
                    break;
                }
            }

            ProcessQueue(); // one last time for the remaining messages
        }

        private void ProcessQueue()
        {
            Record record;
            while (_queue.TryDequeue(out record))
            {
                _pushToTracers(record);
            }
        }
    }
}
