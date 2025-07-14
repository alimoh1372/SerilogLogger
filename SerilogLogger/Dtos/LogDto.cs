using Serilog.Events;

namespace SerilogLogger.Dtos;

public struct LogDto : IDisposable
{
    public LogDto(LogEventLevel logLevel, string logMessage, string methodName, List<KeyValuePair<string, object>>? logParameters = null, Exception? logException = null)
    {
        LogLevel = logLevel;
        LogMessage = logMessage;
        LogException = logException;
        LogParameters = logParameters;
        MethodName = methodName;

        LogParameters?.Add(new("date of create log", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz")));
        LogParameters?.Add(new("ThreadId", Thread.CurrentThread.ManagedThreadId));
    }

    public LogEventLevel LogLevel { get; set; }

    public string LogMessage { get; set; }

    public string MethodName { get; set; }

    public List<KeyValuePair<string, object>>? LogParameters { get; set; }

    public Exception? LogException { get; set; }

    public void Dispose()
    {
        LogMessage = String.Empty;

        if (LogParameters != null)
            LogParameters = null;

        LogException = null;
    }
}