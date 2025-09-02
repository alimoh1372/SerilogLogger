// ===================================
// PropertyDictionary.cs - Complete File
// ===================================
using System.Collections;
using SerilogLogger.Utilities.Utilities;

namespace SerilogLogger.Utilities.Dtos;

/// <summary>
/// Dictionary مخصوص properties با fluent API
/// </summary>
public class PropertyDictionary : Dictionary<string, object?>
{
	public PropertyDictionary() : base() { }

	public PropertyDictionary(int capacity) : base(capacity) { }

	public PropertyDictionary(PropertyDictionary other) : base(other) { }

	public PropertyDictionary(Dictionary<string, object?> properties) : base(properties ?? new Dictionary<string, object?>()) { }

	/// <summary>
	/// اضافه کردن property با syntax ساده
	/// </summary>
	public new PropertyDictionary Add(string key, object? value)
	{
		if (!string.IsNullOrWhiteSpace(key))
		{
			this[key] = value;
		}
		return this;
	}

	/// <summary>
	/// اضافه کردن چندین property
	/// </summary>
	public PropertyDictionary AddRange(Dictionary<string, object?> properties)
	{
		if (properties != null)
		{
			foreach (var kvp in properties)
			{
				if (!string.IsNullOrWhiteSpace(kvp.Key))
				{
					this[kvp.Key] = kvp.Value;
				}
			}
		}
		return this;
	}

	/// <summary>
	/// حذف property با fluent syntax
	/// </summary>
	public new PropertyDictionary Remove(string key)
	{
		base.Remove(key);
		return this;
	}

	/// <summary>
	/// Merge کردن با object دیگر
	/// </summary>
	public PropertyDictionary Merge(object? obj, string? prefix = null)
	{
		if (obj == null) return this;

		// بررسی نوع object برای جلوگیری از فراخوانی غیرضروری
		var objType = obj.GetType();
		if (ObjectFlattener.IsPrimitiveType(objType))
		{
			var key = string.IsNullOrEmpty(prefix) ? "Value" : prefix;
			this[key] = obj;
			return this;
		}

		this.UnionKeyValuePairs(obj, prefix);
		return this;
	}

	/// <summary>
	/// تبدیل به Dictionary عادی
	/// </summary>
	public new Dictionary<string, object?> ToDictionary() => new(this);

	// Static factory methods
	public static PropertyDictionary Create() => new();

	public static PropertyDictionary Create(string key, object? value) => new PropertyDictionary().Add(key, value);

	public static PropertyDictionary From(Dictionary<string, object?> properties) => new(properties);

	/// <summary>
	/// Operator overload برای + (ترکیب دو PropertyDictionary)
	/// </summary>
	public static PropertyDictionary operator +(PropertyDictionary left, PropertyDictionary right)
	{
		var result = new PropertyDictionary(left);
		foreach (var kvp in right)
		{
			result[kvp.Key] = kvp.Value;
		}
		return result;
	}

	/// <summary>
	/// Implicit conversion از tuple
	/// </summary>
	public static implicit operator PropertyDictionary(ValueTuple<string, object?> tuple)
	{
		return new PropertyDictionary { { tuple.Item1, tuple.Item2 } };
	}

	
}