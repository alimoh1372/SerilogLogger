using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Context;
using Serilog.Events;
using SerilogLogger.Dtos.Enums;
using SerilogLogger.LoggerInterface;

namespace SerilogLogger.LoggerImplementation.NormalLog
{
    public class SerilogNormalLogger : ILog
    {
        private readonly string _applicationId;

        private readonly string _applicationName;

        private readonly ILogger _logger;

        const string defaultMessageTemplate = "{MessageTemplate}";

        public SerilogNormalLogger(
            string applicationId,
            string applicationName
            )
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
                .Destructure.ToMaximumCollectionCount(1024)
                .Destructure.ToMaximumDepth(2)
                .Destructure.ToMaximumStringLength(1024)
                 .CreateLogger();

            _logger = Log.Logger;

        }

        public void Debug(
            string messageTemplate,
            List<KeyValuePair<string, object>>? parameters = null,
            Exception? exception = null
        )
        {
            
            SendLog("Debug", messageTemplate, DateTimeOffset.Now, exception,parameters);
        }


        public void Error(
            string messageTemplate,
            List<KeyValuePair<string, object>>? parameters = null,
            Exception? exception = null,
            bool useStackTrace = false
        )
        {

            SendLog("Error", messageTemplate, DateTimeOffset.Now, exception, parameters,useStackTrace);
        }


        public void Fatal(
            string messageTemplate,
            List<KeyValuePair<string, object>>? parameters = null,
            Exception? exception = null
        )
        {
            SendLog("Fatal", messageTemplate, DateTimeOffset.Now, exception, parameters);

        }


        public void Information(
            string messageTemplate,
            List<KeyValuePair<string, object>>? parameters = null,
            Exception? exception = null
        )
        {
            SendLog("Information", messageTemplate, DateTimeOffset.Now, exception, parameters);
        }


        public void Verbose(
            string messageTemplate,
            List<KeyValuePair<string, object>>? parameters = null,
            Exception? exception = null
        )
        {
            SendLog("Verbose", messageTemplate, DateTimeOffset.Now, exception, parameters);
        }


        public void Warning(
            string messageTemplate,
            List<KeyValuePair<string, object>>? parameters = null,
            Exception? exception = null
        )
        {
            SendLog("Warning", messageTemplate, DateTimeOffset.Now, exception, parameters);
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
    }
}
