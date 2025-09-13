using SerilogLogger.Abstraction.Enums;
using SerilogLogger.Abstraction.LoggerInterface;
using SerilogLogger.Utilities.Utilities;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace SerilogLogger.Utilities.LoggerTools;

public static class LogExtensions
{


	/// <summary>
	/// لاگ کردن object با سطح مشخص
	/// </summary>
	public static void LogObject(
		this ILog logger,
		string messageTemplate,
		object? obj,
		string? objectName = null,
		LogLevel level = LogLevel.Debug,
		[CallerMemberName] string methodName = null!,
		[CallerFilePath] string callerPath = null!
		)
	{
		if (obj == null) return;

		// Lazy initialization
		Dictionary<string, object?>? properties = null;

		if (obj != null)
		{
			properties.UnionKeyValuePairs(obj, objectName);
		}

		// فقط در صورت نیاز properties را محاسبه کن
		void EnsureProperties()
		{
			if (properties == null)
			{
				properties = new Dictionary<string, object?>();
				properties.UnionKeyValuePairs(obj, objectName);
			}
		}

		switch (level)
		{
			case LogLevel.Verbose:
				EnsureProperties();
				logger.Verbose(messageTemplate, properties, null, null, methodName, callerPath);
				break;
			case LogLevel.Debug:
				EnsureProperties();
				logger.Debug(messageTemplate, properties, null, null, methodName, callerPath);
				break;
			case LogLevel.Information:
				EnsureProperties();
				logger.Information(messageTemplate, properties, null, null, methodName, callerPath);
				break;
			case LogLevel.Warning:
				EnsureProperties();
				logger.Warning(messageTemplate, properties, null, null, methodName, callerPath);
				break;
			case LogLevel.Error:
				EnsureProperties();
				logger.Error(messageTemplate, properties, null, null, methodName, callerPath);
				break;
			case LogLevel.Fatal:
				EnsureProperties();
				logger.Fatal(messageTemplate, properties, null, null, methodName, callerPath);
				break;
		}

	}

	/// <summary>
	/// Performance logging برای عملیات sync
	/// </summary>
	public static T LogPerformance<T>(this ILog logger,
		Func<T> operation,
		string operationName,
		[CallerMemberName] string callerName = null!,
		[CallerFilePath] string callerPath = null!
		)
	{
		var stopwatch = Stopwatch.StartNew();
		try
		{
			var result = operation();
			stopwatch.Stop();

			logger.Information(
				"Operation completed successfully",
				new Dictionary<string, object?>
				{
					["OperationName"] = operationName,
					["Duration"] = stopwatch.ElapsedMilliseconds,
					["Success"] = true
				},null,null,
				callerName,
				callerPath);

			return result;
		}
		catch (Exception ex)
		{
			stopwatch.Stop();
			logger.Error("Operation failed",
				new Dictionary<string, object?>
			{
				["OperationName"] = operationName,
				["Duration"] = stopwatch.ElapsedMilliseconds,
				["Success"] = false,
				["ErrorMessage"] = ex.Message
			}, ex,
			 null,
			callerName,
			callerPath
				);
			throw;
		}
	}

	/// <summary>
	/// Performance logging برای عملیات async
	/// </summary>
	public static async Task<T> LogPerformanceAsync<T>(this ILog logger, Func<Task<T>> operation, string operationName,
		[CallerMemberName] string callerName = null!,
		[CallerFilePath] string callerPath = null!)
	{
		var stopwatch = Stopwatch.StartNew();
		try
		{
			var result = await operation();
			stopwatch.Stop();

			logger.Information(
				"Async operation completed successfully",
				new Dictionary<string, object?>
			{
				["OperationName"] = operationName,
				["Duration"] = stopwatch.ElapsedMilliseconds,
				["Success"] = true
			},
				null,
				null,
				callerName,
				callerPath);

			return result;
		}
		catch (Exception ex)
		{
			stopwatch.Stop();
			logger.Error("Async operation failed",
				new Dictionary<string, object?>
			{
				["OperationName"] = operationName,
				["Duration"] = stopwatch.ElapsedMilliseconds,
				["Success"] = false,
				["ErrorMessage"] = ex.Message
			},
				ex,
				null,
				callerName,
				callerPath);
			throw;
		}
	}

	/// <summary>
	/// Method call logging
	/// </summary>
	public static T LogMethodCall<T>(this ILog logger, Func<T> method, object? parameters = null,
		[CallerMemberName] string methodName = null!,
		[CallerFilePath] string callerPath = null!)
	{
		var properties = new Dictionary<string, object?>
		{
			["MethodName"] = methodName,
			["StartTime"] = DateTime.UtcNow
		};

		if (parameters != null)
		{
			properties.UnionKeyValuePairs(parameters, "Parameters");
		}

		logger.Debug("Method call started", properties,null,null,methodName,callerPath);

		try
		{
			var result = method();
			properties["EndTime"] = DateTime.UtcNow;
			properties["Success"] = true;
			logger.Debug("Method call completed",properties, null, null, methodName, callerPath);
			return result;
		}
		catch (Exception ex)
		{
			properties["EndTime"] = DateTime.UtcNow;
			properties["Success"] = false;
			properties["ErrorMessage"] = ex.Message;
			logger.Error("Method call failed", properties, ex, null, methodName, callerPath);
			throw;
		}
	}



}