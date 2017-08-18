using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace zipkin4net.Dispatcher
{
    /// <summary>
    /// Dispatch messages asynchronously in order using an ActionBlock
    /// </summary>
    internal class InOrderAsyncActionBlockDispatcher : IRecordDispatcher
    {
        private readonly ActionBlock<Record> _actionBlock;
        private readonly TimeSpan _timeoutOnStop;

        public InOrderAsyncActionBlockDispatcher(Action<Record> pushToTracers, int maxCapacity = 5000, int timeoutOnStopMs = 10000)
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

        public bool Dispatch(Record record)
        {
            return _actionBlock.Post(record);
        }
    }
}
