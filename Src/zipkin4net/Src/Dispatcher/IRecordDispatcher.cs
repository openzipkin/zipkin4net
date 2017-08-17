namespace Criteo.Profiling.Tracing.Dispatcher
{
    internal interface IRecordDispatcher
    {

        void Stop();

        bool Dispatch(Record record);

    }
}
