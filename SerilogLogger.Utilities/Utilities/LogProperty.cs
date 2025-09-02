using System;
using System.Collections.Generic;

namespace SerilogLogger.Utilities.Utilities;

/// <summary>
/// Class برای آسان‌تر کردن ساخت key-value pairs (جایگزین record برای سازگاری بیشتر)
/// </summary>
public class LogProperty
{
	public string Key { get; }
	public object? Value { get; }

	public LogProperty(string key, object? value)
	{
		Key = key ?? throw new ArgumentNullException(nameof(key));
		Value = value;
	}

	public static implicit operator KeyValuePair<string, object?>(LogProperty prop)
		=> new(prop.Key, prop.Value);

	public static implicit operator LogProperty((string Key, object? Value) tuple)
		=> new(tuple.Key, tuple.Value);

	public override bool Equals(object? obj)
	{
		return obj is LogProperty other &&
		       Key == other.Key &&
		       Equals(Value, other.Value);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Key, Value);
	}

	public override string ToString()
	{
		return $"{Key}: {Value}";
	}

	// Deconstruct method برای tuple-like behavior
	public void Deconstruct(out string key, out object? value)
	{
		key = Key;
		value = Value;
	}
}
