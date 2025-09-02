// ===================================
// Extensions/LogPropertyBuilder.cs - Builder pattern
// ===================================
using SerilogLogger.Utilities.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SerilogLogger.Utilities.Utilities;

/// <summary>
/// Builder pattern برای ایجاد PropertyDictionary
/// </summary>
public class LogPropertyBuilder
{
	private readonly PropertyDictionary _properties;

	private LogPropertyBuilder()
	{
		_properties = new PropertyDictionary();
	}

	private LogPropertyBuilder(PropertyDictionary properties)
	{
		_properties = properties;
	}

	/// <summary>
	/// ایجاد LogPropertyBuilder جدید
	/// </summary>
	public static LogPropertyBuilder New() => new LogPropertyBuilder();

	/// <summary>
	/// ایجاد LogPropertyBuilder جدید با property اولیه
	/// </summary>
	public static LogPropertyBuilder New(string key, object? value)
	{
		var builder = new LogPropertyBuilder();
		builder._properties[key] = value;
		return builder;
	}

	/// <summary>
	/// ایجاد LogPropertyBuilder از روی object موجود
	/// </summary>
	public static LogPropertyBuilder From(object? obj, string? prefix = null)
	{
		var builder = new LogPropertyBuilder();
		if (obj != null)
		{
			builder._properties.UnionKeyValuePairs(obj, prefix);
		}
		return builder;
	}

	/// <summary>
	/// اضافه کردن property جدید
	/// </summary>
	public LogPropertyBuilder With(string key, object? value)
	{
		_properties[key] = value;
		return this;
	}

	/// <summary>
	/// اضافه کردن object به عنوان properties
	/// </summary>
	public LogPropertyBuilder WithObject(object? obj, string? prefix = null)
	{
		if (obj != null)
		{
			_properties.UnionKeyValuePairs(obj, prefix);
		}
		return this;
	}

	/// <summary>
	/// تبدیل به PropertyDictionary
	/// </summary>
	public static implicit operator PropertyDictionary(LogPropertyBuilder builder)
	{
		return builder._properties;
	}

	/// <summary>
	/// تبدیل صریح به PropertyDictionary
	/// </summary>
	public PropertyDictionary Build() => _properties;
}