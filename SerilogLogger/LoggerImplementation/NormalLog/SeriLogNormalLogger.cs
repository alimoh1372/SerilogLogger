using Serilog.Events;
using SerilogLogger.LoggerInterface;
using System.Runtime.CompilerServices;

namespace SerilogLogger.LoggerImplementation.NormalLog;

public class SeriLogNormalLogger : BaseSeriLog, ILog
{
    public SeriLogNormalLogger(string applicationId, string applicationName) : base(applicationId, applicationName)
    { }

    public void Debug(string messageTemplate, List<KeyValuePair<string, object>>? parameters = null, Exception? exception = null, [CallerMemberName] string callerName = null!)
        => SendLog(LogEventLevel.Debug, messageTemplate, callerName, exception, parameters);

    public void Error(string messageTemplate, List<KeyValuePair<string, object>>? parameters = null, Exception? exception = null, [CallerMemberName] string callerName = null!)
        => SendLog(LogEventLevel.Error, messageTemplate, callerName, exception, parameters);

    public void Fatal(string messageTemplate, List<KeyValuePair<string, object>>? parameters = null, Exception? exception = null, [CallerMemberName] string callerName = null!)
        => SendLog(LogEventLevel.Fatal, messageTemplate, callerName, exception, parameters);

    public void Information(string messageTemplate, List<KeyValuePair<string, object>>? parameters = null, Exception? exception = null, [CallerMemberName] string callerName = null!)
        => SendLog(LogEventLevel.Information, messageTemplate, callerName, exception, parameters);

    public void Verbose(string messageTemplate, List<KeyValuePair<string, object>>? parameters = null, Exception? exception = null, [CallerMemberName] string callerName = null!)
        => SendLog(LogEventLevel.Verbose, messageTemplate, callerName, exception, parameters);

    public void Warning(string messageTemplate, List<KeyValuePair<string, object>>? parameters = null, Exception? exception = null, [CallerMemberName] string callerName = null!)
        => SendLog(LogEventLevel.Warning, messageTemplate, callerName, exception, parameters);
}