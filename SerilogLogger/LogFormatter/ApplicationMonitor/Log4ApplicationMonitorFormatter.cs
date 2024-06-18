using System.Text;
using Serilog.Events;
using Serilog.Formatting;

namespace SerilogLogger.LogFormatter.ApplicationMonitor;
public class Log4ApplicationMonitorFormatter : ITextFormatter
{
    private static readonly string SourceContextPropertyName = "SourceContext";
    private static readonly string ThreadIdPropertyName = "ThreadId";
    private static readonly string UserNamePropertyName = "EnvironmentUserName";
    private static readonly string DomainPropertyName = "Domain";
    private readonly ApplicationMonitorXmlSerializer xmlSerializer;
    
    public Log4ApplicationMonitorFormatter()
    {
        xmlSerializer = new ApplicationMonitorXmlSerializer();

    }
    
    public void Format(LogEvent logEvent, TextWriter output)
    {

        var stringBuilder = new StringBuilder();


        stringBuilder.Append("<event");
        WriteLogger(logEvent, stringBuilder);
        WriteEventTime(logEvent, stringBuilder);
        WriteLevel(logEvent, stringBuilder);
        WriteThread(logEvent, stringBuilder);
        WriteDomain(logEvent, stringBuilder);
        WriteUserName(logEvent, stringBuilder);
        stringBuilder.Append(">");

        WriteMessage(logEvent, stringBuilder);

        stringBuilder.Append("<properties>");
        WriteProperties(logEvent, stringBuilder);
        stringBuilder.Append("</properties>");


        stringBuilder.Append("</event>");

        output.Write(stringBuilder.ToString());

        stringBuilder.Clear();

        output.Flush();

        output.Close();
    }

    private void WriteLogger(LogEvent logEvent, StringBuilder stringBuilder)
    {
        if (logEvent.Properties.TryGetValue(SourceContextPropertyName, out var sourceContext))
        {
            var sourceContextValue = ((ScalarValue)sourceContext).Value.ToString();
            stringBuilder.Append($" logger=\"{xmlSerializer.SerializeXmlValue(sourceContextValue, true)}\"");
        }
        else stringBuilder.Append(" logger=\"Not Provided\"");
    }

    private static void WriteEventTime(LogEvent logEvent, StringBuilder stringBuilder)
    {
        var timestamp = logEvent.Timestamp;

        var timestampStr = timestamp.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz");

        stringBuilder.Append($" timestamp=\"{timestampStr}\"");
    }

    private static void WriteLevel(LogEvent logEvent, StringBuilder stringBuilder)
    {
        string levelStr = logEvent.Level switch
        {
            LogEventLevel.Debug => "DEBUG",
            LogEventLevel.Information => "INFO",
            LogEventLevel.Warning => "WARN",
            LogEventLevel.Error => "ERROR",
            LogEventLevel.Fatal => "FATAL",
            _ => "TRACE",
        };

        stringBuilder.Append($" level=\"{levelStr}\"");
    }

    private void WriteThread(LogEvent logEvent, StringBuilder stringBuilder)
    {
        if (logEvent.Properties.TryGetValue(ThreadIdPropertyName, out var threadId))
        {
            var threadIdValue = ((ScalarValue)threadId).Value.ToString();
            stringBuilder.Append($" thread=\"{xmlSerializer.SerializeXmlValue(threadIdValue, true)}\"");
        }
        else
        {
            stringBuilder.Append(" thread=\"1\"");
        }
    }

    private void WriteDomain(LogEvent logEvent, StringBuilder stringBuilder)
    {
        if (logEvent.Properties.TryGetValue(DomainPropertyName, out var domain))
        {
            var domainValue = ((ScalarValue)domain).Value.ToString();
            stringBuilder.Append($" domain=\"{xmlSerializer.SerializeXmlValue(domainValue, true)}\"");
        }
        else
        {
            stringBuilder.Append(" domain=\"Not Provided\"");
        }
    }

    private void WriteUserName(LogEvent logEvent, StringBuilder stringBuilder)
    {
        if (logEvent.Properties.TryGetValue(UserNamePropertyName, out var userName))
        {
            var userNameValue = ((ScalarValue)userName).Value.ToString();
            stringBuilder.Append($" username=\"{xmlSerializer.SerializeXmlValue(userNameValue, true)}\"");
        }
        else
        {
            stringBuilder.Append(" username=\"Not Provided\"");
        }
    }

    private void WriteMessage(LogEvent logEvent, StringBuilder stringBuilder)
    {
        stringBuilder.Append("<message>");
        xmlSerializer.SerializeXmlValue(stringBuilder, logEvent.RenderMessage(), false);
        stringBuilder.Append("</message>");
    }

    private void WriteProperties(LogEvent logEvent, StringBuilder stringBuilder)
    {
        foreach (var logEventProperty in logEvent.Properties)
        {
            if (!logEventProperty.Key.Equals(SourceContextPropertyName) &&
                !logEventProperty.Key.Equals(UserNamePropertyName) &&
                !logEventProperty.Key.Equals(DomainPropertyName))
            {
                var sanitizedValue = logEventProperty.Value.ToString().Replace("\"", "").Replace("<", "").Replace(">", "");
                stringBuilder.Append($"<data name=\"{logEventProperty.Key}\" value=\"{sanitizedValue}\" />");
            }
        }

        if (logEvent.Level == LogEventLevel.Fatal || logEvent.Level == LogEventLevel.Error)
        {
            if (logEvent.Exception != null)
            {
                var exceptionDetails = logEvent.Exception;
                var sanitizedMessage = exceptionDetails.Message.Replace('<', '(').Replace('>', ')').Replace('&', ' ');
                var sanitizedSource = exceptionDetails.Source?.Replace('<', '(').Replace('>', ')').Replace('&', ' ');
                var sanitizedStackTrace = exceptionDetails.StackTrace?.Replace('<', '(').Replace('>', ')').Replace('&', ' ');
                var sanitizedInnerException = exceptionDetails.InnerException?.ToString().Replace('<', '(').Replace('>', ')').Replace('&', ' ');

                stringBuilder.Append($"<data name=\"Error Message\" value=\"{sanitizedMessage}\" />");
                stringBuilder.Append($"<data name=\"Error Source\" value=\"{sanitizedSource}\" />");
                stringBuilder.Append($"<data name=\"Error StackTrace\" value=\"{sanitizedStackTrace}\" />");
                stringBuilder.Append($"<data name=\"Error InnerException\" value=\"{sanitizedInnerException}\" />");
            }
        }
    }
}