using System;

namespace zipkin4net.Tracers
{
    /// <summary>
    /// Prints records to the console.
    /// </summary>
    public class ConsoleTracer : ITracer
    {
        public void Record(Record record)
        {
            Console.WriteLine(record);
        }
    }
}
