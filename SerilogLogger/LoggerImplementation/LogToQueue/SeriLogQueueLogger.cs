using System.Collections.Concurrent;
using Serilog.Events;
using SerilogLogger.Abstraction.LoggerInterface;
using SerilogLogger.Implementation.Dtos;

namespace SerilogLogger.Implementation.LoggerImplementation.LogToQueue;

public class SeriLogQueueLogger : BaseSeriLog, ILog
{
    private BlockingCollection<LogDto> _logs = new();

    public SeriLogQueueLogger(string applicationId, string applicationName) : base(applicationId, applicationName)
        => Task.Run(ProcessorLogs);

    private void ProcessorLogs()
    {
        foreach (var log in _logs.GetConsumingEnumerable())
            using (log)
                SendLog(log.LogLevel, log.LogMessage, log.MethodName, log.LogException, log.LogParameters);
    }

    public void Debug(string messageTemplate, List<KeyValuePair<string, object>>? parameters = null, Exception? exception = null, [CallerMemberName] string callerName = null!)
        => _logs.Add(new LogDto(LogEventLevel.Debug, messageTemplate, callerName, parameters, exception));

    public void Error(string messageTemplate, List<KeyValuePair<string, object>>? parameters = null, Exception? exception = null, [CallerMemberName] string callerName = null!)
        => _logs.Add(new LogDto(LogEventLevel.Error, messageTemplate, callerName, parameters, exception));

    public void Fatal(string messageTemplate, List<KeyValuePair<string, object>>? parameters = null, Exception? exception = null, [CallerMemberName] string callerName = null!)
        => _logs.Add(new LogDto(LogEventLevel.Fatal, messageTemplate, callerName, parameters, exception));

    public void Information(string messageTemplate, List<KeyValuePair<string, object>>? parameters = null, Exception? exception = null, [CallerMemberName] string callerName = null!)
        => _logs.Add(new LogDto(LogEventLevel.Information, messageTemplate, callerName, parameters, exception));

    public void Verbose(string messageTemplate, List<KeyValuePair<string, object>>? parameters = null, Exception? exception = null, [CallerMemberName] string callerName = null!)
        => _logs.Add(new LogDto(LogEventLevel.Verbose, messageTemplate, callerName, parameters, exception));

    public void Warning(string messageTemplate, List<KeyValuePair<string, object>>? parameters = null, Exception? exception = null, [CallerMemberName] string callerName = null!)
        => _logs.Add(new LogDto(LogEventLevel.Warning, messageTemplate, callerName, parameters, exception));
}
