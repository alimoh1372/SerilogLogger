using Serilog;
using SerilogLogger.Abstraction.LoggerInterface;

namespace SerilogLogger.Implementation.Tools.LoggerFactoryContext;

public interface ISerilogLoggerFactory
{
	ILog CreateLogger(string loggerType, ILogger serilogLogger);
}