using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Context;
using SerilogLogger.LoggerInterface;

namespace SerilogLogger.LoggerImplementation.NormalLog
{
    public class SerilogNormalLogger : ILog
    {
        private readonly string _applicationId;

        private readonly string _applicationName;

        private readonly ILogger _logger;

        public SerilogNormalLogger(
            string applicationId,
            string applicationName
            )
        {
            _applicationId = applicationId;

            _applicationName = applicationName;

            var loggConfiguration = new ConfigurationBuilder()
                .AddJsonFile("LogConfiguration.json",
                    optional: true, reloadOnChange: true)
                 .Build();

            _logger = new LoggerConfiguration()
                 .ReadFrom.Configuration(loggConfiguration)
                 .CreateLogger();

        }

        public void Debug(
            string messageTemplate,
            List<KeyValuePair<string, object>>? parameters = null,
            Exception? exception = null
        )
        {
            SendLog("Debug", messageTemplate, exception, parameters);
        }


        public void Error(
            string messageTemplate,
            List<KeyValuePair<string, object>>? parameters = null,
            Exception? exception = null,
            bool useStackTrace = false
        )
        {
            SendLog("Error", messageTemplate, exception, parameters, useStackTrace);
        }


        public void Fatal(
            string messageTemplate,
            List<KeyValuePair<string, object>>? parameters = null,
            Exception? exception = null
        )
        {
            SendLog("Fatal", messageTemplate, exception, parameters);
        }


        public void Information(
            string messageTemplate,
            List<KeyValuePair<string, object>>? parameters = null,
            Exception? exception = null
        )
        {
            SendLog("Information", messageTemplate, exception, parameters);
        }


        public void Verbose(
            string messageTemplate,
            List<KeyValuePair<string, object>>? parameters = null,
            Exception? exception = null
        )
        {
            SendLog("Verbose", messageTemplate, exception, parameters);
        }


        public void Warning(
            string messageTemplate,
            List<KeyValuePair<string, object>>? parameters = null,
            Exception? exception = null
        )
        {
            SendLog("Warning", messageTemplate, exception, parameters);
        }


        //public void QueueLog(
        //    string level,
        //    string messageTemplate,
        //    List<KeyValuePair<string, object>>? parameters = null,
        //    Exception? exception = null
        //)
        //{
        //    SendLog(level, messageTemplate, exception, parameters);
        //}


        private void SendLog(
            string level,
            string messageTemplate,
            Exception? exception,
            List<KeyValuePair<string, object>>? parameters,
            bool useStackTrace = false
            )
        {

            parameters ??= new List<KeyValuePair<string, object>>();


            parameters.Add(new KeyValuePair<string, object>("EventOccurredTime",
                DateTimeOffset.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz")));

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
}
