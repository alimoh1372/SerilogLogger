// ===================================
// PropertyDictionaryExtensions.cs - Complete File
// ===================================

using SerilogLogger.Utilities.Dtos;
using SerilogLogger.Utilities.Utilities;

namespace SerilogLogger.Utilities.LoggerTools;

public static class PropertyDictionaryExtensions
{
	/// <summary>
	/// تبدیل object به PropertyDictionary
	/// </summary>
	public static PropertyDictionary ToPropertyDictionary(this object? obj, string? prefix = null)
	{
		var dict = new PropertyDictionary();
		if (obj != null)
		{
			dict.UnionKeyValuePairs(obj, prefix);
		}
		return dict;
	}

	/// <summary>
	/// اضافه کردن object به PropertyDictionary موجود
	/// </summary>
	public static PropertyDictionary AddObject(this PropertyDictionary dictionary, object? obj, string? prefix = null)
	{
		if (obj != null)
		{
			dictionary.UnionKeyValuePairs(obj, prefix);
		}
		return dictionary;
	}

	/// <summary>
	/// اضافه کردن conditional property
	/// </summary>
	public static PropertyDictionary AddIf(this PropertyDictionary dictionary, bool condition, string key, object? value)
	{
		if (condition && !string.IsNullOrWhiteSpace(key))
		{
			dictionary[key] = value;
		}
		return dictionary;
	}

	/// <summary>
	/// اضافه کردن property فقط اگر value null نباشد
	/// </summary>
	public static PropertyDictionary AddIfNotNull(this PropertyDictionary dictionary, string key, object? value)
	{
		if (value != null && !string.IsNullOrWhiteSpace(key))
		{
			dictionary[key] = value;
		}
		return dictionary;
	}

	/// <summary>
	/// اضافه کردن property با formatting
	/// </summary>
	public static PropertyDictionary AddFormatted(this PropertyDictionary dictionary, string key, string format, params object?[] args)
	{
		if (!string.IsNullOrWhiteSpace(key))
		{
			dictionary[key] = string.Format(format, args);
		}
		return dictionary;
	}

	public static PropertyDictionary ToPropertyDictionary(this Dictionary<string, object?> dict)
		=> new (dict);

	public static Dictionary<string, object?> ToDictionaryCopy(this PropertyDictionary pd)
		=> new (pd);
}