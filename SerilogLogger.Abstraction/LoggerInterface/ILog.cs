// ===================================
// ILog.cs - Enhanced Interface
// ===================================
using System.Runtime.CompilerServices;
using SerilogLogger.Abstraction.Dtos;

namespace SerilogLogger.Abstraction.LoggerInterface;

public interface ILog
{
	public void Verbose(
		string messageTemplate,
		Dictionary<string, object?>? properties = null,
		Exception? exception = null,
		LoggingOptions? options = null,
		[CallerMemberName] string methodName = null!,
		[CallerFilePath] string callerPath = null!
		);

	void Debug(
		string messageTemplate,
		Dictionary<string, object?>? properties = null,
		Exception? exception = null,
		LoggingOptions? options = null,
		[CallerMemberName] string methodName = null!,
		[CallerFilePath] string callerPath = null!
		);

	void Information(
		string messageTemplate,
		Dictionary<string, object?>? properties = null,
		Exception? exception = null,
		LoggingOptions? options = null,
		[CallerMemberName] string methodName = null!,
		[CallerFilePath] string callerPath = null!);

	void Warning(
		string messageTemplate,
		Dictionary<string, object?>? properties = null,
		Exception? exception = null,
		LoggingOptions? options = null,
		[CallerMemberName] string methodName = null!,
		[CallerFilePath] string callerPath = null!);

	void Error(string messageTemplate,
		Dictionary<string, object?>? properties = null,
		Exception? exception = null,
		LoggingOptions? options = null,
		[CallerMemberName] string methodName = null!,
		[CallerFilePath] string callerPath = null!);

	void Fatal(
		string messageTemplate,
		Dictionary<string, object?>? properties = null,
		Exception? exception = null,
		LoggingOptions? options = null,
		[CallerMemberName] string methodName = null!,
		[CallerFilePath] string callerPath = null!);
}