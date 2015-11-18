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

        public InOrderAsyncDispatcher(Action<Record> pushToTracers)
        {
            _actionBlock = new ActionBlock<Record>(pushToTracers,
                      new ExecutionDataflowBlockOptions
                      {
                          MaxDegreeOfParallelism = 1
                      });
        }

        public void Stop()
        {
            _actionBlock.Complete();
            _actionBlock.Completion.Wait();
        }

        public void Dispatch(Record record)
        {
            _actionBlock.Post(record);
        }

    }
}
