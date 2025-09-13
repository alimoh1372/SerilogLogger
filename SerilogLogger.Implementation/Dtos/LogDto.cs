// ===================================
// Dtos/OptimizedLogDto.cs - Memory efficient log DTO
// ===================================
using Serilog.Events;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SerilogLogger.Implementation.Dtos;

/// <summary>
/// بهینه‌سازی شده برای کاهش allocation و بهبود performance
/// </summary>
/// <summary>
/// High-performance, memory-efficient log data transfer object
/// Uses struct to avoid heap allocation for small logs
/// </summary>
[StructLayout(LayoutKind.Auto)] // Let compiler optimize layout
public readonly struct LogDto : IEquatable<LogDto>
{
	public readonly LogEventLevel Level;
	public readonly string Message;
	public readonly string? CallerName;
	public readonly string? CallerPath;
	public readonly Exception? Exception;
	public readonly IReadOnlyDictionary<string, object?>? Properties;
	public readonly DateTime Timestamp;
	public readonly int ThreadId;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public LogDto(
		LogEventLevel level,
		string message,
		string? callerName = null,
		string? callerPath = null,
		IReadOnlyDictionary<string, object?>? properties = null,
		Exception? exception = null)
	{
		Level = level;
		Message = message ?? throw new ArgumentNullException(nameof(message));
		CallerName = callerName;
		CallerPath = callerPath;
		Properties = properties;
		Exception = exception;
		Timestamp = DateTime.UtcNow;
		ThreadId = Thread.CurrentThread.ManagedThreadId;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static LogDto Create(
		LogEventLevel level,
		string message,
		Dictionary<string, object?>? properties = null,
		Exception? exception = null,
		[CallerMemberName] string? callerName = null,
		[CallerFilePath] string? callerPath = null)
	{
		return new LogDto(level, message, callerName, callerPath, properties, exception);
	}

	// Efficient property access
	public bool HasException => Exception != null;
	public bool HasProperties => Properties != null && Properties.Count > 0;
	public bool HasCaller => !string.IsNullOrEmpty(CallerName);

	// File name only (performance optimization)
	public string? CallerFileName => string.IsNullOrEmpty(CallerPath)
		? null
		: Path.GetFileName(CallerPath);

	// IEquatable implementation for better performance in collections
	public bool Equals(LogDto other)
	{
		return Level == other.Level &&
			   Message == other.Message &&
			   CallerName == other.CallerName &&
			   CallerPath == other.CallerPath &&
			   Timestamp.Equals(other.Timestamp) &&
			   ThreadId == other.ThreadId;
	}

	public override bool Equals(object? obj)
	{
		return obj is LogDto other && Equals(other);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Level, Message, CallerName, Timestamp, ThreadId);
	}

	public static bool operator ==(LogDto left, LogDto right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(LogDto left, LogDto right)
	{
		return !left.Equals(right);
	}

	public override string ToString()
	{
		return $"[{Level}] {Message} ({CallerName})";
	}
}