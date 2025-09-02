// ===================================
// LogBuilder.cs - Complete File
// ===================================
using SerilogLogger.Abstraction.Dtos;
using SerilogLogger.Abstraction.LoggerInterface;
using SerilogLogger.Utilities.Dtos;
using System.Runtime.CompilerServices;

namespace SerilogLogger.Utilities.LoggerTools;

/// <summary>
/// Fluent API Builder برای ساخت لاگ‌ها
/// </summary>
public class LogBuilder
{
	private readonly ILog _logger;
	private string _messageTemplate = string.Empty;
	private PropertyDictionary _properties = new();
	private Exception? _exception;
	private LoggingOptions? _options;
	private string _callerName = null!;
	private string _callerPath = null!;

	internal LogBuilder(ILog logger)
	{
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public LogBuilder Message(string messageTemplate)
	{
		_messageTemplate = messageTemplate ?? throw new ArgumentNullException(nameof(messageTemplate));
		return this;
	}

	public LogBuilder WithProperty(string key, object? value)
	{
		_properties.Add(key, value);
		return this;
	}

	public LogBuilder WithProperties(PropertyDictionary properties)
	{
		if (properties != null)
		{
			foreach (var kvp in properties)
			{
				_properties[kvp.Key] = kvp.Value;
			}
		}
		return this;
	}

	public LogBuilder WithProperties(Dictionary<string, object?> properties)
	{
		if (properties != null)
		{
			foreach (var kvp in properties)
			{
				if (!string.IsNullOrWhiteSpace(kvp.Key))
				{
					_properties[kvp.Key] = kvp.Value;
				}
			}
		}
		return this;
	}

	public LogBuilder WithException(Exception exception)
	{
		_exception = exception;
		return this;
	}

	public LogBuilder WithOptions(LoggingOptions options)
	{
		_options = options;
		return this;
	}

	public LogBuilder WithOptions(Action<LoggingOptions> configureOptions)
	{
		var options = new LoggingOptions();
		configureOptions(options);
		_options = options;
		return this;
	}

	public LogBuilder From([CallerMemberName] string callerName = null!, [CallerFilePath] string callerPath = null!)
	{
		_callerName = callerName;
		_callerPath = callerPath;
		return this;
	}

	public void AsVerbose() => _logger.Verbose(_messageTemplate, _properties.ToDictionary(), _exception, _options, _callerName, _callerPath);
	public void AsDebug() => _logger.Debug(_messageTemplate, _properties.ToDictionary(), _exception, _options, _callerName, _callerPath);
	public void AsInformation() => _logger.Information(_messageTemplate, _properties.ToDictionary(), _exception, _options, _callerName, _callerPath);
	public void AsWarning() => _logger.Warning(_messageTemplate, _properties.ToDictionary(), _exception, _options, _callerName, _callerPath);
	public void AsError() => _logger.Error(_messageTemplate, _properties.ToDictionary(), _exception, _options, _callerName, _callerPath);
	public void AsFatal() => _logger.Fatal(_messageTemplate, _properties.ToDictionary(), _exception, _options, _callerName, _callerPath);
}