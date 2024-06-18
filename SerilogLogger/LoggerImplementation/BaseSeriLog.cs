using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Context;
using Serilog.Events;

namespace SerilogLogger.LoggerImplementation;

public class BaseSeriLog
{
    protected readonly string _applicationId;

    protected readonly string _applicationName;

    protected readonly ILogger _logger;

    protected BaseSeriLog(string applicationId, string applicationName)
    {
        _applicationId = applicationId;

        _applicationName = applicationName;

        var logConfiguration = new ConfigurationBuilder()
            .AddJsonFile("LogConfiguration.json",
                optional: true, reloadOnChange: true)
            .Build();

        _logger = new LoggerConfiguration()
            .ReadFrom.Configuration(logConfiguration)
            .Enrich.WithProperty("Hostname", Environment.MachineName)
            .Enrich.WithProperty("UserName", Environment.UserName)
            // the "Domain" Enrich is for application monitor
            .Enrich.WithProperty("Domain", _applicationName)
            .Enrich.WithProperty("ApplicationName", _applicationName)
            .Enrich.WithProperty("ApplicationId", _applicationId)
            .CreateLogger();
    }

    protected void SendLog(LogEventLevel logEventLevel, string messageTemplate, Exception? exception, List<KeyValuePair<string, object>>? parameters)
    {
        if (parameters is not null)
        {
            var disposables = new IDisposable[parameters.Count];

            for (int i = 0; i < parameters.Count; i++)
                disposables[i] = LogContext.PushProperty(parameters[i].Key, parameters[i].Value);

            CheckLevel(logEventLevel, messageTemplate, exception, parameters);

            foreach (var disposable in disposables) disposable.Dispose();

        }
        else CheckLevel(logEventLevel, messageTemplate, exception, parameters);
        
        LogContext.Reset();
    }

    private void CheckLevel(LogEventLevel logEventLevel, string messageTemplate, Exception? exception, List<KeyValuePair<string, object>>? parameters)
    {
        switch (logEventLevel)
        {
            case LogEventLevel.Information:
                _logger.Information(exception, messageTemplate);
                break;
            case LogEventLevel.Verbose:
                _logger.Verbose(exception, messageTemplate);
                break;
            case LogEventLevel.Warning:
                _logger.Warning(exception, messageTemplate);
                break;
            case LogEventLevel.Debug:
                _logger.Debug(exception, messageTemplate);
                break;
            case LogEventLevel.Fatal:
                _logger.Fatal(exception, messageTemplate);
                break;
            case LogEventLevel.Error:
                _logger.Error(exception, messageTemplate);
                break;
        }
    }
}
