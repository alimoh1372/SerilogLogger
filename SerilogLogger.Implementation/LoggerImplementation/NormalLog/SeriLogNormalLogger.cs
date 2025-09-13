// ===================================
// SeriLogNormalLogger.cs - Enhanced Implementation
// ===================================
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;
using SerilogLogger.Abstraction.Dtos;
using SerilogLogger.Abstraction.LoggerInterface;

namespace SerilogLogger.Implementation.LoggerImplementation.NormalLog;

public class SeriLogNormalLogger : BaseSeriLog, ILog
{
	public SeriLogNormalLogger(ILogger logger, IOptions<ApplicationLogConfiguration> config) : base(logger, config) { }

	public void Verbose(
		string messageTemplate,
		Dictionary<string, object?>? properties = null,
		Exception? exception = null,
		LoggingOptions? options = null,
		[CallerMemberName] string methodName = null!,
		[CallerFilePath] string callerPath = null!)
	{
		if (string.IsNullOrWhiteSpace(messageTemplate))
			throw new ArgumentException("Message template cannot be null or empty", nameof(messageTemplate));

		SendLog(LogEventLevel.Verbose, messageTemplate, methodName, callerPath, exception, properties, options);
	}

	public void Debug(
		string messageTemplate,
		Dictionary<string, object?>? properties = null,
		Exception? exception = null,
		LoggingOptions? options = null,
		[CallerMemberName] string methodName = null!,
		[CallerFilePath] string callerPath = null!)
	{
		if (string.IsNullOrWhiteSpace(messageTemplate))
			throw new ArgumentException("Message template cannot be null or empty", nameof(messageTemplate));

		SendLog(LogEventLevel.Debug, messageTemplate, methodName, callerPath, exception, properties, options);
	}

	public void Information(
		string messageTemplate,
		Dictionary<string, object?>? properties = null,
		Exception? exception = null,
		LoggingOptions? options = null,
		[CallerMemberName] string methodName = null!,
		[CallerFilePath] string callerPath = null!)
	{
		if (string.IsNullOrWhiteSpace(messageTemplate))
			throw new ArgumentException("Message template cannot be null or empty", nameof(messageTemplate));

		SendLog(LogEventLevel.Information, messageTemplate, methodName, callerPath, exception, properties, options);
	}

	public void Warning(
		string messageTemplate,
		Dictionary<string, object?>? properties = null,
		Exception? exception = null,
		LoggingOptions? options = null,
		[CallerMemberName] string methodName = null!,
		[CallerFilePath] string callerPath = null!)
	{
		if (string.IsNullOrWhiteSpace(messageTemplate))
			throw new ArgumentException("Message template cannot be null or empty", nameof(messageTemplate));

		SendLog(LogEventLevel.Warning, messageTemplate, methodName, callerPath, exception, properties, options);
	}

	public void Error(
		string messageTemplate,
		Dictionary<string, object?>? properties = null,
		Exception? exception = null,
		LoggingOptions? options = null,
		[CallerMemberName] string methodName = null!,
		[CallerFilePath] string callerPath = null!)
	{
		if (string.IsNullOrWhiteSpace(messageTemplate))
			throw new ArgumentException("Message template cannot be null or empty", nameof(messageTemplate));

		SendLog(LogEventLevel.Error, messageTemplate, methodName, callerPath, exception, properties, options);
	}

	public void Fatal(
		string messageTemplate,
		Dictionary<string, object?>? properties = null,
		Exception? exception = null,
		LoggingOptions? options = null,
		[CallerMemberName] string methodName = null!,
		[CallerFilePath] string callerPath = null!)
	{
		if (string.IsNullOrWhiteSpace(messageTemplate))
			throw new ArgumentException("Message template cannot be null or empty", nameof(messageTemplate));

		SendLog(LogEventLevel.Fatal, messageTemplate, methodName, callerPath, exception, properties, options);
	}


}