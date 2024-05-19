using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Context;
using Serilog.Core;
using Serilog.Events;
using SerilogLogger.Dtos;
using SerilogLogger.Dtos.Enums;
using SerilogLogger.LoggerInterface;

namespace SerilogLogger.LoggerImplementation.LogToQueue;

public class SerilogQueueLogger : ILog
{

    private readonly string _applicationId;

    private readonly string _applicationName;

    private readonly ILogger _logger;

    private ConcurrentQueue<LogQueueDto> _logQueue = new();

    private DateTime lastClearedDataWithGCCollector = DateTime.Now;

    private DateTime lastClearedQueueTime = DateTime.Now;

    private readonly object _lockObject = new object();

    const string defaultMessageTemplate = "{MessageTemplate}";

    public SerilogQueueLogger(string applicationId, string applicationName)
    {
        _applicationId = applicationId;

        _applicationName = applicationName;

        var logConfiguration = new ConfigurationBuilder()
            .AddJsonFile("LogConfiguration.json",
                optional: true, reloadOnChange: true)
            .Build();


        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(logConfiguration)
            .Enrich.WithProperty("hostName", Environment.MachineName)
            .Enrich.WithProperty("EnvironmentUserName", Environment.UserName)
            .Enrich.WithProperty("Domain", _applicationName)
            .Enrich.WithProperty("ApplicationId", _applicationId)
            .Enrich.WithProperty("ApplicationId", _applicationId)
            .Enrich.WithProperty("ApplicationName", _applicationName)
            .CreateLogger();

        _logger = Log.Logger;

        Task.Run(LogQueueProcessor);

    }


    private void SendLog(string level,
      string messageTemplate,
      DateTimeOffset logTime,
      Exception? exception,
      List<KeyValuePair<string, object>>? parameters,
      bool useStackTrace = false)
    {
        try
        {
            parameters ??= new List<KeyValuePair<string, object>>();


            parameters.Add(new KeyValuePair<string, object>("EventOccurredTime",
                logTime.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz")));

            parameters.Add(new("Timespan", DateTimeOffset.Now));

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

            ILogger tempLogger = _logger.ForContext(
                 "MessageTemplate",
                 messageTemplate
             );

            foreach (var (key, value) in parameters)
            {

                tempLogger = tempLogger.ForContext(key, value);

            }


            switch (level)
            {
                case "Debug":
                    tempLogger.Debug(exception, defaultMessageTemplate);
                    break;
                case "Error":
                    tempLogger.Error(exception, defaultMessageTemplate);
                    break;
                case "Fatal":
                    tempLogger.Fatal(exception, defaultMessageTemplate);
                    break;
                case "Information":
                    tempLogger.Information(exception, defaultMessageTemplate);
                    break;
                case "Verbose":
                    tempLogger.Verbose(exception, defaultMessageTemplate);
                    break;
                case "Warning":
                    tempLogger.Warning(exception, defaultMessageTemplate);
                    break;
            }

            tempLogger = null;
           
        }
        catch (Exception ex)
        {
            _logger.Error(ex, defaultMessageTemplate, $"Error Occurred in this solution.ErrorMessage:{ex.Message}");
        }

    }
    private void LogQueueProcessor()
    {

        // this while for checking always queue size
        while (true)
        {
            //برای این مورد باید روش بهتری باشه
            List<LogQueueDto> tempQueue = new();

            if ((DateTime.Now - lastClearedQueueTime).TotalSeconds > 2 * 60)
            {
                lock (_lockObject)
                {
                    if (_logQueue.IsEmpty == false)
                    {
                        tempQueue = _logQueue.ToList();
                    }

                    _logQueue.Clear();
                    _logQueue = new();
                }

                if (tempQueue.Any())
                {
                    foreach (LogQueueDto logQueueDto in tempQueue)
                    {
                        _logQueue.Enqueue(logQueueDto);
                    }
                }

                tempQueue.Clear();

                lastClearedQueueTime = DateTime.Now;
            }
            var elapsedSeconds = (DateTime.Now - lastClearedDataWithGCCollector).TotalSeconds;

            if (elapsedSeconds > 2 * 60)
            {
                GC.Collect();

                lastClearedDataWithGCCollector = DateTime.Now;
            }

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

        Log.CloseAndFlushAsync();
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



}