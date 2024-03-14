using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Context;
using SerilogLogger.Dtos;
using SerilogLogger.Dtos.Enums;
using SerilogLogger.LoggerInterface;

namespace SerilogLogger.LoggerImplementation.LogToQueue;

public class SerilogQueueLogger : ILog
{

    private readonly string _applicationId;

    private readonly string _applicationName;

    private readonly ILogger _logger;

    private readonly ConcurrentQueue<LogQueueDto> _logQueue = new();


    public SerilogQueueLogger(string applicationId, string applicationName)
    {
        _applicationId = applicationId;

        _applicationName = applicationName;

        var logConfiguration = new ConfigurationBuilder()
            .AddJsonFile("LogConfiguration.json",
                optional: true, reloadOnChange: true)
            .Build();

        _logger = new LoggerConfiguration()
            .ReadFrom.Configuration(logConfiguration)
            .CreateLogger();

        Task.Run(LogQueueProcessor);
    }

    private void LogQueueProcessor()
    {

        // this while for checking always queue size
        while (true)
            while (_logQueue.IsEmpty is false && _logQueue.TryDequeue(out var log))
                SendLog(
                        log.LogLevel.ToString(),
                        log.LogMessage,
                        log.LogTime,
                        log.LogException,
                        log.LogParameters,
                        log.IsStackTraceEnable
                        );
    }

    private void AddLogToQueue(LogQueueDto logQueueDto)
    {
        _logQueue.Enqueue(logQueueDto);
    }
    public void Debug(
        string messageTemplate,
        List<KeyValuePair<string, object>>? parameters = null,
        Exception? exception = null
    )
    {
        AddLogToQueue(new LogQueueDto(ELogLevel.Debug, messageTemplate, parameters, exception));
    }

    public void Error(
        string messageTemplate,
        List<KeyValuePair<string, object>>? parameters = null,
        Exception? exception = null,
        bool useStackTrace = false
    )
    {
        AddLogToQueue(new LogQueueDto(ELogLevel.Error, messageTemplate, parameters, exception, useStackTrace));
    }


    public void Fatal(
        string messageTemplate,
        List<KeyValuePair<string, object>>? parameters = null,
        Exception? exception = null
    )
    {
        AddLogToQueue(new LogQueueDto(ELogLevel.Fatal, messageTemplate, parameters, exception));
    }

    public void Information(
        string messageTemplate,
        List<KeyValuePair<string, object>>? parameters = null,
        Exception? exception = null
    )
    {
        AddLogToQueue(new LogQueueDto(ELogLevel.Information, messageTemplate, parameters, exception));
    }

    public void Verbose(
        string messageTemplate,
        List<KeyValuePair<string, object>>? parameters = null,
        Exception? exception = null
    )
    {
        AddLogToQueue(new LogQueueDto(ELogLevel.Verbose, messageTemplate, parameters, exception));
    }


    public void Warning(
        string messageTemplate,
        List<KeyValuePair<string, object>>? parameters = null,
        Exception? exception = null
    )
    {
        AddLogToQueue(new LogQueueDto(ELogLevel.Warning, messageTemplate, parameters, exception));
    }


    private void SendLog(string level,
        string messageTemplate,
        DateTimeOffset logTime,
        Exception? exception,
        List<KeyValuePair<string, object>>? parameters,
        bool useStackTrace = false)
    {

        parameters ??= new List<KeyValuePair<string, object>>();

        parameters.Add(new KeyValuePair<string, object>("EventOccurredTime",
            logTime.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz")));

        parameters.Add(new("hostname",
            Environment.MachineName));

        parameters.Add(new("EnvironmentUserName",
            Environment.UserName));

        parameters.Add(new("Domain",
            _applicationName));

        parameters.Add(new("ApplicationId",
            _applicationId));

        parameters.Add(new("ApplicationName",
            _applicationName));

        if (useStackTrace && exception != null)
        {
            var stackTrace = new StackTrace(exception);

            var temp = new StringBuilder();

            for (var i = 0; i < stackTrace.GetFrames().Length; i++)
            {
                temp.Append("the error has occurred in file line ");
                temp.Append($"number -> {stackTrace.GetFrames()[i].GetFileLineNumber()} ");
                temp.Append($"in file name -> {stackTrace.GetFrames()[i].GetFileName()} ");
                temp.Append($"in method name -> {stackTrace.GetFrames()[i].GetMethod()}");
                temp.Append("\n");

                parameters.Add(
                    new($"stack trace error list index-{i}",
                        temp.ToString())
                );
            }

            temp.Clear();
        }

        foreach (var (key, value) in parameters)
            LogContext.PushProperty(key, value);


        switch (level)
        {
            case "Debug":
                _logger.Debug(exception, messageTemplate);
                break;
            case "Error":
                _logger.Error(exception, messageTemplate);
                break;
            case "Fatal":
                _logger.Fatal(exception, messageTemplate);
                break;
            case "Information":
                _logger.Information(exception, messageTemplate);
                break;
            case "Verbose":
                _logger.Verbose(exception, messageTemplate);
                break;
            case "Warning":
                _logger.Warning(exception, messageTemplate);
                break;
        }

        LogContext.Reset();
    }
}