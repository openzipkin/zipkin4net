namespace zipkin4net
{
    public interface ITracer
    {
        void Record(Record record);
    }
}
