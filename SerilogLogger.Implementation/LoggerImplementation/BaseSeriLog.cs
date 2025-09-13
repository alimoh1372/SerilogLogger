// ===================================
// BaseSeriLog.cs - Enhanced Base Class
// ===================================
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;
using Serilog.Context;
using SerilogLogger.Implementation.Tools;
using SerilogLogger.Abstraction.Dtos;

namespace SerilogLogger.Implementation.LoggerImplementation;

public abstract class BaseSeriLog
{
	protected readonly ILogger Logger;
	protected readonly ApplicationLogConfiguration Config;

	protected BaseSeriLog(ILogger logger, IOptions<ApplicationLogConfiguration> config)
	{
		Logger = logger ?? throw new ArgumentNullException(nameof(logger));
		Config = config is null? throw new ArgumentNullException(nameof(config))
				:config.Value;
	}

	protected void SendLog(LogEventLevel logEventLevel, string messageTemplate, string methodName, string callerPath,
		Exception? exception, Dictionary<string, object?>? parameters, LoggingOptions? options = null)
	{
		if (!Logger.IsEnabled(logEventLevel))
			return;

		using var performanceTracker = CreatePerformanceTracker(options);

		using var disposeLogProperties = new DisposeLogProperties();

		try
		{
			// Add basic context
			AddBasicContext(methodName, callerPath, disposeLogProperties);

			// Process parameters with enhanced features
			ProcessEnhancedParameters(parameters, disposeLogProperties, options);

			// Process exception with configuration
			if (Config.EnableExceptionDetails)
			{
				ProcessException(exception, disposeLogProperties);
			}

			// Add method call tracking if enabled
			AddMethodCallTracking(methodName, callerPath, disposeLogProperties, options);

			// Add additional properties from options
			AddAdditionalProperties(options?.AdditionalProperties, disposeLogProperties);

			Logger.Write(logEventLevel, exception, messageTemplate);
		}
		catch (Exception ex)
		{
			SafeLogError(ex, messageTemplate);
		}
	}

	private PerformanceTracker? CreatePerformanceTracker(LoggingOptions? options)
	{
		var enablePerformance = options?.EnablePerformanceLogging ?? Config.EnablePerformanceLogging;

		if (!enablePerformance) return null;

		return new PerformanceTracker("LogOperation", (operation, elapsed) =>
		{
			if (elapsed > 100) // Only log slow operations
			{
				Logger.Warning("Slow logging operation: {Operation} took {ElapsedMs}ms", operation, elapsed);
			}
		});
	}

	private void AddBasicContext(string methodName, string callerPath, DisposeLogProperties disposeLogProperties)
	{
		if (Config.EnableContextualProperties)
		{
			string className = GetCachedClassName(callerPath);
			disposeLogProperties.Add(LogContext.PushProperty("ClassName", className));
			disposeLogProperties.Add(LogContext.PushProperty("MethodName", methodName));
		}
	}

	private void AddMethodCallTracking(string methodName, string callerPath, DisposeLogProperties disposeLogProperties, LoggingOptions? options)
	{
		var enableTracking = options?.EnableMethodCallTracking ?? Config.EnableMethodCallTracking;

		if (enableTracking)
		{
			disposeLogProperties.Add(LogContext.PushProperty("CallStack", Environment.StackTrace));
			disposeLogProperties.Add(LogContext.PushProperty("ThreadId", Environment.CurrentManagedThreadId));
			disposeLogProperties.Add(LogContext.PushProperty("Timestamp", DateTimeOffset.UtcNow));
		}
	}

	private void ProcessEnhancedParameters(Dictionary<string, object?>? parameters, DisposeLogProperties disposeLogProperties, LoggingOptions? options)
	{
		if (parameters?.Count > 0)
		{
			foreach (var kvp in parameters)
			{
				if (!string.IsNullOrWhiteSpace(kvp.Key))
				{
					var processedValue = ObjectProcessor.ProcessObject(kvp.Value, Config, options);
					disposeLogProperties.Add(LogContext.PushProperty(kvp.Key, processedValue, destructureObjects: true));
				}
			}
		}
	}

	private void AddAdditionalProperties(Dictionary<string, object?>? additionalProperties, DisposeLogProperties disposeLogProperties)
	{
		if (additionalProperties?.Count > 0)
		{
			foreach (var kvp in additionalProperties)
			{
				if (!string.IsNullOrWhiteSpace(kvp.Key))
				{
					var processedValue = ObjectProcessor.ProcessObject(kvp.Value, Config);
					disposeLogProperties.Add(LogContext.PushProperty($"Additional_{kvp.Key}", processedValue, destructureObjects: true));
				}
			}
		}
	}

	// Cache برای className ها
	private static readonly Dictionary<string, string> _classNameCache = new(StringComparer.OrdinalIgnoreCase);

	private static string GetCachedClassName(string callerPath)
	{
		if (_classNameCache.TryGetValue(callerPath, out var cached))
			return cached;

		var className = Path.GetFileNameWithoutExtension(callerPath) ?? "UnknownClass";
		_classNameCache[callerPath] = className;
		return className;
	}

	private void ProcessException(Exception? exception, DisposeLogProperties disposeLogProperties)
	{
		if (exception == null) return;

		disposeLogProperties.Add(LogContext.PushProperty("ExceptionType", exception.GetType().FullName));
		disposeLogProperties.Add(LogContext.PushProperty("ExceptionMessage", exception.Message));

		if (!string.IsNullOrEmpty(exception.StackTrace))
		{
			disposeLogProperties.Add(LogContext.PushProperty("StackTrace", exception.StackTrace));
		}

		ProcessExceptionData(exception, disposeLogProperties);
		ProcessInnerException(exception.InnerException, disposeLogProperties);
	}

	private void ProcessExceptionData(Exception exception, DisposeLogProperties disposeLogProperties)
	{
		if (exception.Data.Count == 0) return;

		var exData = new Dictionary<string, object?>(exception.Data.Count);
		foreach (var key in exception.Data.Keys)
		{
			if (key?.ToString() is { } keyStr && !string.IsNullOrEmpty(keyStr))
			{
				exData[keyStr] = exception.Data[key];
			}
		}

		if (exData.Count > 0)
		{
			disposeLogProperties.Add(LogContext.PushProperty("ExceptionData", exData, destructureObjects: true));
		}
	}

	private void ProcessInnerException(Exception? innerException, DisposeLogProperties disposeLogProperties)
	{
		if (innerException == null) return;

		disposeLogProperties.Add(LogContext.PushProperty("InnerExceptionType", innerException.GetType().FullName));
		disposeLogProperties.Add(LogContext.PushProperty("InnerExceptionMessage", innerException.Message));

		if (!string.IsNullOrEmpty(innerException.StackTrace))
		{
			disposeLogProperties.Add(LogContext.PushProperty("InnerExceptionStackTrace", innerException.StackTrace));
		}
	}

	private void SafeLogError(Exception ex, string messageTemplate)
	{
		try
		{
			Logger.Error(ex, "Logging failed for template: {MessageTemplate}", messageTemplate);
		}
		catch
		{
			Console.Error.WriteLine($"Critical logging failure: {ex.Message}");
		}
	}
}
