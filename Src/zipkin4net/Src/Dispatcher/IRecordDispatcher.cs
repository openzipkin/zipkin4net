namespace zipkin4net.Dispatcher
{
    public interface IRecordDispatcher
    {
        /// <summary>
        /// Stops the dispatcher.
        /// </summary>
        void Stop();
        /// <summary>
        /// Dispatches a record to the registered tracers.
        /// </summary>
        /// <returns>True if the record was dispatched. Otherwise false.</returns>
        bool Dispatch(Record record);

    }
}
