// ===================================
// Utilities/ObjectFlattener.cs - ابزار flatten objects
// ===================================

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;


namespace SerilogLogger.Utilities.Utilities;

public static class ObjectFlattener
{
	private static readonly ConcurrentDictionary<Type, PropertyInfo[]> PropertyCache = new();

	private const int MaxDepth = 5;

	private const int MaxCollectionItems = 10;

	public static void UnionKeyValuePairs(this Dictionary<string, object?> dictionary, object? obj, string? prefix = null, int depth = 0)
	{
		if (obj == null || depth > MaxDepth) return;

		try
		{
			var objType = obj.GetType();

			// Handle primitive types and strings
			if (IsPrimitiveType(objType))
			{
				var key = string.IsNullOrEmpty(prefix) ? "Value" : prefix;
				dictionary[key] = obj;
				return;
			}

			// Handle collections
			if (obj is IEnumerable enumerable && objType != typeof(string))
			{
				HandleCollection(dictionary, enumerable, prefix, depth);
				return;
			}

			// Handle complex objects
			HandleComplexObject(dictionary, obj, objType, prefix, depth);
		}
		catch (Exception)
		{
			// Silent failure - add fallback value
			var key = string.IsNullOrEmpty(prefix) ? "Value" : prefix;
			dictionary[key] = obj?.ToString() ?? "null";
		}
	}

	private static void HandleCollection(Dictionary<string, object?> dictionary, IEnumerable enumerable, string? prefix, int depth)
	{
		var items = enumerable.Cast<object?>().Take(MaxCollectionItems).ToList();

		var basePrefix = string.IsNullOrEmpty(prefix) ? "Item" : prefix;

		for (int i = 0; i < items.Count; i++)
		{
			var itemKey = $"{basePrefix}[{i}]";
			dictionary.UnionKeyValuePairs(items[i], itemKey, depth + 1);
		}

		if (items.Count >= MaxCollectionItems)
		{
			dictionary[$"{basePrefix}_HasMore"] = true;
			dictionary[$"{basePrefix}_TotalCount"] = enumerable.Cast<object?>().Count();
		}
	}

	// بهبود HandleComplexObject برای عملکرد بهتر
	private static void HandleComplexObject(Dictionary<string, object?> dictionary, object obj, Type objType, string? prefix, int depth)
	{
		// بررسی اولیه برای جلوگیری از پردازش غیرضروری
		if (depth >= MaxDepth) return;

		var properties = PropertyCache.GetOrAdd(objType, type =>
		{
			return type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
				.Where(p => p.CanRead &&
				            p.GetIndexParameters().Length == 0 && // فقط properties بدون index
				            !p.GetCustomAttributes<LogIgnoreAttribute>().Any())
				.ToArray();
		});

		// پردازش موازی برای objects پیچیده
		if (properties.Length > 10)
		{
			var tasks = properties.Select(property => new { property, value = GetPropertyValue(obj, property) })
				.Where(x => x.value != null);

			foreach (var item in tasks)
			{
				var key = string.IsNullOrEmpty(prefix) ? item.property.Name : $"{prefix}.{item.property.Name}";
				dictionary.UnionKeyValuePairs(item.value, key, depth + 1);
			}
		}
		else
		{
			foreach (var property in properties)
			{
				var value = GetPropertyValue(obj, property);
				if (value != null)
				{
					var key = string.IsNullOrEmpty(prefix) ? property.Name : $"{prefix}.{property.Name}";
					dictionary.UnionKeyValuePairs(value, key, depth + 1);
				}
			}
		}
	}

	private static object? GetPropertyValue(object obj, PropertyInfo property)
	{
		try
		{
			return property.GetValue(obj);
		}
		catch
		{
			return null; // Skip problematic properties
		}
	}

	// اضافه کردن method public برای IsPrimitiveType در ObjectFlattener.cs
	public static bool IsPrimitiveType(Type type)
	{
		return type.IsPrimitive ||
		       type == typeof(string) ||
		       type == typeof(decimal) ||
		       type == typeof(DateTime) ||
		       type == typeof(DateTimeOffset) ||
		       type == typeof(TimeSpan) ||
		       type == typeof(Guid) ||
		       type.IsEnum ||
		       (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) &&
		        IsPrimitiveType(Nullable.GetUnderlyingType(type)!));
	}

}