namespace zipkin4net
{
    public interface ILogger
    {
        void LogInformation(string message);
        void LogWarning(string message);
        void LogError(string message);
    }
}

