
// ===================================
// FluentILogExtensions.cs - Complete File
// ===================================
using SerilogLogger.Utilities.Dtos;
using System.Runtime.CompilerServices;
using SerilogLogger.Abstraction.LoggerInterface;
using SerilogLogger.Abstraction.Dtos;

namespace SerilogLogger.Utilities.LoggerTools;

/// <summary>
/// Extension methods برای ILog با Fluent API
/// </summary>
public static class FluentILogExtensions
{
	#region Basic PropertyDictionary Extensions

	public static void Verbose(this ILog logger, string messageTemplate, PropertyDictionary properties,
		LoggingOptions? options = null,
		[CallerMemberName] string callerName = null!, [CallerFilePath] string callerPath = null!)
	{
		logger.Verbose(messageTemplate,  properties.ToDictionary(), null, options);
	}

	public static void Debug(
		this ILog logger, string messageTemplate,
		PropertyDictionary properties,
		LoggingOptions? options = null,
		[CallerMemberName] string callerName = null!,
		[CallerFilePath] string callerPath = null!)
	{
		logger.Debug(messageTemplate, properties.ToDictionary(), null, options,callerName,callerPath);
	}

	public static void Information(this ILog logger, string messageTemplate,
		PropertyDictionary properties,
		LoggingOptions? options = null,
		[CallerMemberName] string callerName = null!,
		[CallerFilePath] string callerPath = null!)
	{
		logger.Information(messageTemplate, properties.ToDictionary(), null, options, callerName, callerPath);
	}

	public static void Warning(this ILog logger, string messageTemplate, PropertyDictionary properties,
		[CallerMemberName] string callerName = null!, [CallerFilePath] string callerPath = null!,
		LoggingOptions? options = null)
	{
		logger.Warning(messageTemplate, properties.ToDictionary(), null, options, callerName, callerPath);
	}

	public static void Error(this ILog logger, string messageTemplate, PropertyDictionary properties, Exception? exception = null,
		[CallerMemberName] string callerName = null!, [CallerFilePath] string callerPath = null!,
		LoggingOptions? options = null)
	{
		logger.Error(messageTemplate, properties.ToDictionary(), null, options, callerName, callerPath);
	}

	public static void Fatal(this ILog logger, string messageTemplate, PropertyDictionary properties, Exception? exception = null,
		[CallerMemberName] string callerName = null!, [CallerFilePath] string callerPath = null!,
		LoggingOptions? options = null)
	{
		logger.Fatal(messageTemplate, properties.ToDictionary(), null, options, callerName, callerPath);
	}

	#endregion

	#region Backward Compatibility Extensions (without PropertyDictionary)

	public static void Verbose(this ILog logger, string messageTemplate,
		[CallerMemberName] string callerName = null!, [CallerFilePath] string callerPath = null!,
		LoggingOptions? options = null)
	{
		logger.Verbose(messageTemplate, null, null, options, callerName, callerPath);
	}

	public static void Debug(this ILog logger, string messageTemplate,
		[CallerMemberName] string callerName = null!, [CallerFilePath] string callerPath = null!,
		LoggingOptions? options = null)
	{
		logger.Debug(messageTemplate, null, null, options, callerName, callerPath);
	}

	public static void Information(this ILog logger, string messageTemplate,
		[CallerMemberName] string callerName = null!, [CallerFilePath] string callerPath = null!,
		LoggingOptions? options = null)
	{
		logger.Information(messageTemplate, null, null, options, callerName, callerPath);
	}

	public static void Warning(this ILog logger, string messageTemplate,
		[CallerMemberName] string callerName = null!, [CallerFilePath] string callerPath = null!,
		LoggingOptions? options = null)
	{
		logger.Warning(messageTemplate, null, null, options, callerName, callerPath);
	}

	public static void Error(this ILog logger, string messageTemplate, Exception? exception = null,
		[CallerMemberName] string callerName = null!, [CallerFilePath] string callerPath = null!,
		LoggingOptions? options = null)
	{
		logger.Error(messageTemplate, null, null, options, callerName, callerPath);
	}

	public static void Fatal(this ILog logger, string messageTemplate, Exception? exception = null,
		[CallerMemberName] string callerName = null!, [CallerFilePath] string callerPath = null!,
		LoggingOptions? options = null)
	{
		logger.Fatal(messageTemplate, null, null, options, callerName, callerPath
		
		);
	}

	#endregion

	#region Fluent Builder Extensions

	/// <summary>
	/// شروع Fluent API برای لاگ
	/// </summary>
	public static LogBuilder StartLog(this ILog logger) => new(logger);

	/// <summary>
	/// Helper برای ایجاد PropertyDictionary به صورت Fluent
	/// </summary>
	public static PropertyDictionary Properties() => new PropertyDictionary();

	/// <summary>
	/// Helper برای ایجاد PropertyDictionary با یک property
	/// </summary>
	public static PropertyDictionary Properties(string key, object? value) => new PropertyDictionary().Add(key, value);

	/// <summary>
	/// Helper برای ایجاد PropertyDictionary از Dictionary
	/// </summary>
	public static PropertyDictionary Properties(Dictionary<string, object?> properties) => new PropertyDictionary(properties);

	#endregion
}