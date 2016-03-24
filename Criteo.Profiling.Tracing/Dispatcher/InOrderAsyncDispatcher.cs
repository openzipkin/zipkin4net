using System;
using System.Threading.Tasks.Dataflow;

namespace Criteo.Profiling.Tracing.Dispatcher
{
    /// <summary>
    /// Dispatch messages asynchronously in order
    /// </summary>
    internal class InOrderAsyncDispatcher : IRecordDispatcher
    {
        private readonly ActionBlock<Record> _actionBlock;
        private const int MaxCapacity = 5000;

        public InOrderAsyncDispatcher(Action<Record> pushToTracers)
        {
            _actionBlock = new ActionBlock<Record>(pushToTracers,
                      new ExecutionDataflowBlockOptions
                      {
                          MaxDegreeOfParallelism = 1,
                          BoundedCapacity = MaxCapacity
                      });
        }

        public void Stop()
        {
            _actionBlock.Complete();
            _actionBlock.Completion.Wait();
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
