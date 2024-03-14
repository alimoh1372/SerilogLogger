
namespace SerilogLogger.LoggerInterface
{
    public interface ILog
    {
      
        public void Verbose(
            string messageTemplate,
            List<KeyValuePair<string, object>>? parameters = null,
            Exception? exception = null
        );

        public void Debug(
            string messageTemplate,
            List<KeyValuePair<string, object>>? parameters = null,
            Exception? exception = null
        );

        public void Information(
            string messageTemplate,
            List<KeyValuePair<string, object>>? parameters = null,
            Exception? exception = null
        );

        public void Warning(
            string messageTemplate,
            List<KeyValuePair<string, object>>? parameters = null,
            Exception? exception = null
        );

        public void Error(
            string messageTemplate,
            List<KeyValuePair<string, object>>? parameters = null,
            Exception? exception = null,
            bool useStackTrace = false
        );

        public void Fatal(
            string messageTemplate,
            List<KeyValuePair<string, object>>? parameters = null,
            Exception? exception = null
            );


    }
}
