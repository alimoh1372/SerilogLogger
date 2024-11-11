using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Context;
using Serilog.Events;
using SerilogLogger.Utilities;

namespace SerilogLogger.LoggerImplementation;

public class BaseSeriLog
{
    protected readonly string ApplicationId;

    protected readonly string ApplicationName;

    protected readonly ILogger Logger;

    protected BaseSeriLog(string applicationId, string applicationName)
    {
        ApplicationId = applicationId;

        ApplicationName = applicationName;

        var logConfiguration = new ConfigurationBuilder()
            .AddJsonFile("LogConfiguration.json",
                optional: true, reloadOnChange: true)
            .Build();

        Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(logConfiguration)
            .Enrich.WithProperty("Hostname", Environment.MachineName)
            .Enrich.WithProperty("UserName", Environment.UserName)
            // the "Domain" Enrich is for application monitor
            .Enrich.WithProperty("Domain", ApplicationName)
            .Enrich.WithProperty("ApplicationName", ApplicationName)
            .Enrich.WithProperty("ApplicationId", ApplicationId)
            .CreateLogger();
    }

    protected void SendLog(LogEventLevel logEventLevel, string messageTemplate, Exception? exception, List<KeyValuePair<string, object>>? parameters)
    {
        using (var disposeLogProperties = new DisposeLogProperties())
        {
            if (parameters is not null)
                foreach (var parameter in parameters)
                    disposeLogProperties.Add(LogContext.PushProperty(parameter.Key, parameter.Value));

            Logger.Write(logEventLevel, exception, messageTemplate);
        }

        LogContext.Reset();
    }
}
