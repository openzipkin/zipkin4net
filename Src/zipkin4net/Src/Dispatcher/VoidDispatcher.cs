namespace zipkin4net.Dispatcher
{
    internal class VoidDispatcher : IRecordDispatcher
    {
        public void Stop()
        {
        }

        public bool Dispatch(Record record)
        {
            // Throw away
            return true;
        }
    }
}
