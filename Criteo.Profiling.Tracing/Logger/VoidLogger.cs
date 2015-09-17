namespace Criteo.Profiling.Tracing.Logger
{

    class VoidLogger : ILogger
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
