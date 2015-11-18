namespace Criteo.Profiling.Tracing.Dispatcher
{
    internal class VoidDispatcher : IRecordDispatcher
    {
        public void Stop()
        {
        }

        public void Dispatch(Record record)
        {
            // Throw away
        }
    }
}
