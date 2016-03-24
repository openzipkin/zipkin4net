using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Criteo.Profiling.Tracing.Dispatcher
{
    /// <summary>
    /// Dispatch messages asynchronously in order
    /// </summary>
    internal class InOrderAsyncDispatcher : IRecordDispatcher
    {
        private readonly ActionBlock<Record> _actionBlock;
        private readonly TimeSpan _stopTimeout;

        public InOrderAsyncDispatcher(Action<Record> pushToTracers, int maxCapacity = 5000, int stopTimeoutMs = 10000)
        {
            _actionBlock = new ActionBlock<Record>(pushToTracers,
                      new ExecutionDataflowBlockOptions
                      {
                          MaxDegreeOfParallelism = 1,
                          BoundedCapacity = maxCapacity
                      });
            _stopTimeout = TimeSpan.FromMilliseconds(stopTimeoutMs);
        }

        public void Stop()
        {
            _actionBlock.Complete();
            Task.WaitAny(_actionBlock.Completion, Task.Delay(_stopTimeout));
        }

        public void Dispatch(Record record)
        {
            if (!_actionBlock.Post(record))
            {
                TraceManager.Configuration.Logger.LogWarning("Couldn't dispatch record, actor may be blocked by another operation");
            }
        }

    }
}
