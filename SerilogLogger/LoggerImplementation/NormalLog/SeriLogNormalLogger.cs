using Serilog.Events;
using SerilogLogger.LoggerInterface;

namespace SerilogLogger.LoggerImplementation.NormalLog;

public class SeriLogNormalLogger : BaseSeriLog, ILog
{
    public SeriLogNormalLogger(string applicationId, string applicationName) : base(applicationId, applicationName)
    { }

    public void Debug(string messageTemplate, List<KeyValuePair<string, object>>? parameters = null, Exception? exception = null)
        => SendLog(LogEventLevel.Debug, messageTemplate, exception, parameters);

    public void Error(string messageTemplate, List<KeyValuePair<string, object>>? parameters = null, Exception? exception = null)
        => SendLog(LogEventLevel.Error, messageTemplate, exception, parameters);

    public void Fatal(string messageTemplate, List<KeyValuePair<string, object>>? parameters = null, Exception? exception = null)
        => SendLog(LogEventLevel.Fatal, messageTemplate, exception, parameters);

    public void Information(string messageTemplate, List<KeyValuePair<string, object>>? parameters = null, Exception? exception = null)
        => SendLog(LogEventLevel.Information, messageTemplate, exception, parameters);

    public void Verbose(string messageTemplate, List<KeyValuePair<string, object>>? parameters = null, Exception? exception = null)
        => SendLog(LogEventLevel.Verbose, messageTemplate, exception, parameters);

    public void Warning(string messageTemplate, List<KeyValuePair<string, object>>? parameters = null, Exception? exception = null)
        => SendLog(LogEventLevel.Warning, messageTemplate, exception, parameters);
}