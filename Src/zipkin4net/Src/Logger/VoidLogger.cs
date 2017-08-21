namespace zipkin4net.Logger
{
    internal class VoidLogger : ILogger
    {
        public void LogInformation(string message)
        {
            // NO-OP
        }

        public void LogWarning(string message)
        {
            // NO-OP
        }

        public void LogError(string message)
        {
            // NO-OP
        }
    }
}
