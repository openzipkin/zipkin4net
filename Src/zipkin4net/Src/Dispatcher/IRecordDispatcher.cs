namespace zipkin4net.Dispatcher
{
    internal interface IRecordDispatcher
    {

        void Stop();

        bool Dispatch(Record record);

    }
}
