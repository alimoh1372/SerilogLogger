using System.Collections.Generic;

namespace SerilogLogger.Utilities.Utilities;

/// <summary>
/// Record برای آسان‌تر کردن ساخت key-value pairs
/// </summary>
public record LogPropertyRecord(string Key, object? Value)
{
	public static implicit operator KeyValuePair<string, object?>(LogPropertyRecord prop)
		=> new(prop.Key, prop.Value);

	public static implicit operator LogPropertyRecord((string Key, object? Value) tuple)
		=> new(tuple.Key, tuple.Value);

	public string Key { get; } = Key;
	public object? Value { get; } = Value;
}