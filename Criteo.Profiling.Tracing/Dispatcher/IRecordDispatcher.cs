namespace Criteo.Profiling.Tracing.Dispatcher
{
    internal interface IRecordDispatcher
    {

        void Stop();

        void Dispatch(Record record);

    }
}
