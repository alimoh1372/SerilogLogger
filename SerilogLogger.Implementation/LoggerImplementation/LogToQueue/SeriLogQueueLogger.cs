// ===================================
// SeriLogQueueLogger.cs - Fixed Queue Implementation
// ===================================

using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;
using SerilogLogger.Abstraction.Dtos;
using SerilogLogger.Abstraction.LoggerInterface;

namespace SerilogLogger.Implementation.LoggerImplementation.LogToQueue;

public class SeriLogQueueLogger : BaseSeriLog, ILog, IDisposable
{
	private readonly ConcurrentQueue<LogEntry> _logQueue;
	private readonly Timer _flushTimer;
	private readonly CancellationTokenSource _cancellationTokenSource;
	private readonly Task _processingTask;

	public SeriLogQueueLogger(ILogger logger, IOptions<ApplicationLogConfiguration>  config) : base(logger, config)
	{
		_logQueue = new ConcurrentQueue<LogEntry>();

		_cancellationTokenSource = new CancellationTokenSource();

		_flushTimer = new Timer(FlushLogs, null, config.Value.AsyncFlushInterval, config.Value.AsyncFlushInterval);

		_processingTask = Task.Run(ProcessLogQueue, _cancellationTokenSource.Token);
	}

	

	public void Verbose(
		string messageTemplate,
		Dictionary<string, object?>? properties = null,
		Exception? exception = null,
		LoggingOptions? options = null,
		[CallerMemberName] string methodName = "",
		[CallerFilePath] string callerPath = ""
	)
	{
		EnqueueLog(LogEventLevel.Verbose, messageTemplate, properties, exception, options, methodName, callerPath);
	}
	public void Debug(string messageTemplate, Dictionary<string, object?>? parameters = null, Exception? exception = null,
		LoggingOptions? options = null, [CallerMemberName] string methodName = null!, [CallerFilePath] string callerPath = null!)
	{
		EnqueueLog(LogEventLevel.Debug, messageTemplate, parameters, exception, options, methodName, callerPath);
	}

	public void Information(string messageTemplate, Dictionary<string, object?>? parameters = null, Exception? exception = null,
		LoggingOptions? options = null, [CallerMemberName] string methodName = null!, [CallerFilePath] string callerPath = null!)
	{
		EnqueueLog(LogEventLevel.Information, messageTemplate, parameters, exception, options, methodName, callerPath);
	}

	public void Warning(string messageTemplate, Dictionary<string, object?>? parameters = null, Exception? exception = null,
		LoggingOptions? options = null, [CallerMemberName] string methodName = null!, [CallerFilePath] string callerPath = null!)
	{
		EnqueueLog(LogEventLevel.Warning, messageTemplate, parameters, exception, options, methodName, callerPath);
	}

	public void Error(string messageTemplate, Dictionary<string, object?>? parameters = null, Exception? exception = null,
		LoggingOptions? options = null, [CallerMemberName] string methodName = null!, [CallerFilePath] string callerPath = null!)
	{
		EnqueueLog(LogEventLevel.Error, messageTemplate, parameters, exception, options, methodName, callerPath);
	}

	public void Fatal(string messageTemplate, Dictionary<string, object?>? parameters = null, Exception? exception = null,
		LoggingOptions? options = null, [CallerMemberName] string methodName = null!, [CallerFilePath] string callerPath = null!)
	{
		EnqueueLog(LogEventLevel.Fatal, messageTemplate, parameters, exception, options, methodName, callerPath);
	}

	private void EnqueueLog(LogEventLevel level, string messageTemplate, Dictionary<string, object?>? parameters,
		Exception? exception, LoggingOptions? options, string callerName, string callerPath)
	{
		if (string.IsNullOrWhiteSpace(messageTemplate))
			throw new ArgumentException("Message template cannot be null or empty", nameof(messageTemplate));

		if (_logQueue.Count >= Config.AsyncBufferSize)
		{
			// If queue is full, process synchronously to prevent memory issues
			SendLog(level, messageTemplate, callerName, callerPath, exception, parameters, options);
			return;
		}

		var logEntry = new LogEntry
		{
			Level = level,
			MessageTemplate = messageTemplate,
			// Fix: Create new dictionary instead of casting
			Parameters = parameters != null ? new Dictionary<string, object?>(parameters) : null,
			Exception = exception,
			Options = options,
			CallerName = callerName,
			CallerPath = callerPath,
			Timestamp = DateTimeOffset.UtcNow
		};

		_logQueue.Enqueue(logEntry);
	}

	private async Task ProcessLogQueue()
	{
		while (!_cancellationTokenSource.Token.IsCancellationRequested)
		{
			try
			{
				ProcessQueuedLogs();
				await Task.Delay(50, _cancellationTokenSource.Token); // Small delay to prevent busy waiting
			}
			catch (OperationCanceledException)
			{
				break;
			}
			catch (Exception ex)
			{
				// Log the error but continue processing
				try
				{
					Logger.Error(ex, "Error processing log queue");
				}
				catch
				{
					// Last resort
					Console.Error.WriteLine($"Critical error in log queue processing: {ex.Message}");
				}
			}
		}

		// Process remaining logs on shutdown
		ProcessQueuedLogs();
	}

	private void ProcessQueuedLogs()
	{
		while (_logQueue.TryDequeue(out var logEntry))
		{
			try
			{
				SendLog(logEntry.Level, logEntry.MessageTemplate, logEntry.CallerName, logEntry.CallerPath,
					logEntry.Exception, logEntry.Parameters, logEntry.Options);
			}
			catch (Exception ex)
			{
				// If we can't log this entry, at least try to log the error
				try
				{
					Logger.Error(ex, "Failed to process queued log entry: {MessageTemplate}", logEntry.MessageTemplate);
				}
				catch
				{
					// Complete failure - write to console
					Console.Error.WriteLine($"Critical failure processing log: {ex.Message}");
				}
			}
		}
	}

	private void FlushLogs(object? state)
	{
		ProcessQueuedLogs();
	}

	public void Dispose()
	{
		_flushTimer?.Dispose();
		_cancellationTokenSource.Cancel();

		try
		{
			_processingTask.Wait(TimeSpan.FromSeconds(5));
		}
		catch (Exception ex)
		{
			Console.Error.WriteLine($"Error during logger disposal: {ex.Message}");
		}

		_cancellationTokenSource.Dispose();
	}

	private class LogEntry
	{
		public LogEventLevel Level { get; set; }
		public string MessageTemplate { get; set; } = string.Empty;
		public Dictionary<string, object?>? Parameters { get; set; }
		public Exception? Exception { get; set; }
		public LoggingOptions? Options { get; set; }
		public string CallerName { get; set; } = string.Empty;
		public string CallerPath { get; set; } = string.Empty;
		public DateTimeOffset Timestamp { get; set; }
	}



}