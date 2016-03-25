using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Criteo.Profiling.Tracing.Utils;

namespace Criteo.Profiling.Tracing.Dispatcher
{
    /// <summary>
    /// Dispatch messages asynchronously in order
    /// </summary>
    internal class InOrderAsyncDispatcher : IRecordDispatcher
    {
        private const int MinimumTimeBetweenLogsMin = 1;

        private readonly ActionBlock<Record> _actionBlock;
        private readonly TimeSpan _timeoutOnStop;
        private DateTime _lastLoggedMessage = default(DateTime);

        public InOrderAsyncDispatcher(Action<Record> pushToTracers, int maxCapacity = 5000, int timeoutOnStopMs = 10000)
        {
            _actionBlock = new ActionBlock<Record>(pushToTracers,
                      new ExecutionDataflowBlockOptions
                      {
                          MaxDegreeOfParallelism = 1,
                          BoundedCapacity = maxCapacity
                      });
            _timeoutOnStop = TimeSpan.FromMilliseconds(timeoutOnStopMs);
        }

        public void Stop()
        {
            _actionBlock.Complete();
            Task.WaitAny(_actionBlock.Completion, Task.Delay(_timeoutOnStop));
        }

        public void Dispatch(Record record)
        {
            if (!_actionBlock.Post(record))
            {
                var now = TimeUtils.UtcNow;
                if (_lastLoggedMessage == default(DateTime) || now.Subtract(_lastLoggedMessage).TotalMinutes > MinimumTimeBetweenLogsMin)
                {
                    TraceManager.Configuration.Logger.LogWarning("Couldn't dispatch record, actor may be blocked by another operation");
                    _lastLoggedMessage = now;
                }
            }
        }

    }
}
