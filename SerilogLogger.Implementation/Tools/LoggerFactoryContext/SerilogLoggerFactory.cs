// ===================================
// SerilogLoggerFactory.cs - Factory Pattern for logger creation
// ===================================

using Microsoft.Extensions.DependencyInjection;
using Serilog;
using SerilogLogger.Abstraction.LoggerInterface;
using SerilogLogger.Implementation.LoggerImplementation.LogToQueue;
using SerilogLogger.Implementation.LoggerImplementation.NormalLog;

namespace SerilogLogger.Implementation.Tools.LoggerFactoryContext;

public class SerilogLoggerFactory : ISerilogLoggerFactory
{
	private readonly IServiceProvider _serviceProvider;
	private readonly Dictionary<string, Type> _loggerTypes;

	public SerilogLoggerFactory(IServiceProvider serviceProvider)
	{
		_serviceProvider = serviceProvider;
		_loggerTypes = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
		{
			{ "Normal", typeof(SeriLogNormalLogger) },
			{ "Queue", typeof(SeriLogQueueLogger) }
		};
	}

	public ILog CreateLogger(string loggerType, ILogger serilogLogger)
	{
		if (string.IsNullOrWhiteSpace(loggerType))
		{
			return ActivatorUtilities.CreateInstance<SeriLogNormalLogger>(_serviceProvider, serilogLogger);
		}

		if (_loggerTypes.TryGetValue(loggerType, out var loggerTypeImpl))
		{
			return (ILog)ActivatorUtilities.CreateInstance(_serviceProvider, loggerTypeImpl, serilogLogger);
		}

		return ActivatorUtilities.CreateInstance<SeriLogNormalLogger>(_serviceProvider, serilogLogger);
	}

	public void RegisterLoggerType(string loggerTypeName, Type loggerType)
	{
		if (!typeof(ILog).IsAssignableFrom(loggerType))
		{
			throw new ArgumentException($"Type {loggerType.Name} must implement ILog", nameof(loggerType));
		}
		_loggerTypes[loggerTypeName] = loggerType;
	}
}