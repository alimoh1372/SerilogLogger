// ===================================
// Extensions/FluentLoggerExtensions.cs - Fluent API
// ===================================
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using SerilogLogger.Utilities.Dtos;

namespace SerilogLogger.Utilities.Utilities;

/// <summary>
/// Fluent API برای راحت‌تر کردن استفاده از logger
/// </summary>
public static class FluentLoggerExtensions
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
	/// تبدیل tuple array به PropertyDictionary
	/// </summary>
	public static PropertyDictionary ToPropertyDictionary(this (string Key, object? Value)[] tuples)
	{
		var dict = new PropertyDictionary();
		foreach (var (key, value) in tuples)
		{
			dict[key] = value;
		}
		return dict;
	}

	/// <summary>
	/// تبدیل tuple enumerable به PropertyDictionary
	/// </summary>
	public static PropertyDictionary ToPropertyDictionary(this IEnumerable<(string Key, object? Value)> tuples)
	{
		var dict = new PropertyDictionary();
		foreach (var (key, value) in tuples)
		{
			dict[key] = value;
		}
		return dict;
	}

	/// <summary>
	/// ایجاد PropertyDictionary خالی
	/// </summary>
	public static PropertyDictionary Props() => new PropertyDictionary();

	/// <summary>
	/// ایجاد PropertyDictionary با یک property
	/// </summary>
	public static PropertyDictionary Props(string key, object? value) => new PropertyDictionary { { key, value } };

	/// <summary>
	/// اضافه کردن property جدید با key/value
	/// </summary>
	public static PropertyDictionary With(this PropertyDictionary dict, string key, object? value)
	{
		dict[key] = value;
		return dict;
	}

	/// <summary>
	/// اضافه کردن object با prefix اختیاری
	/// </summary>
	public static PropertyDictionary WithObject(this PropertyDictionary dict, object? obj, string? prefix = null)
	{
		if (obj != null)
		{
			dict.UnionKeyValuePairs(obj, prefix);
		}
		return dict;
	}

	/// <summary>
	/// اضافه کردن anonymous object بدون prefix
	/// </summary>
	public static PropertyDictionary WithProps(this PropertyDictionary dict, object anonymousObject)
	{
		dict.UnionKeyValuePairs(anonymousObject);
		return dict;
	}
}