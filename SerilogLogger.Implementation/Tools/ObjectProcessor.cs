
// ===================================
// ObjectProcessor.cs - Object processing utility
// ===================================
using System.Collections;
using System.Reflection;
using SerilogLogger.Abstraction.Dtos;

namespace SerilogLogger.Implementation.Tools;

public static class ObjectProcessor
{
	public static object? ProcessObject(object? obj, ApplicationLogConfiguration config, LoggingOptions? options = null)
	{
		if (obj == null) return null;

		var maxDepth = options?.MaxObjectDepth ?? config.MaxObjectDepth;
		var maxCollectionItems = options?.MaxCollectionItems ?? config.MaxCollectionItems;

		return ProcessObjectInternal(obj, maxDepth, maxCollectionItems, 0);
	}

	private static object? ProcessObjectInternal(object? obj, int maxDepth, int maxCollectionItems, int currentDepth)
	{
		if (obj == null || currentDepth >= maxDepth) return obj;

		var objType = obj.GetType();

		// Handle primitive types and strings
		if (objType.IsPrimitive || obj is string || obj is DateTime || obj is decimal)
			return obj;

		// Handle collections
		if (obj is IEnumerable enumerable and not string)
		{
			return ProcessCollection(enumerable, maxDepth, maxCollectionItems, currentDepth);
		}

		// Handle complex objects
		if (currentDepth < maxDepth - 1)
		{
			return ProcessComplexObject(obj, maxDepth, maxCollectionItems, currentDepth);
		}

		return obj.ToString();
	}

	private static object ProcessCollection(IEnumerable enumerable, int maxDepth, int maxCollectionItems, int currentDepth)
	{
		var items = new List<object?>();
		var count = 0;

		foreach (var item in enumerable)
		{
			if (maxCollectionItems > 0 && count >= maxCollectionItems)
			{
				items.Add($"... and {GetRemainingCount(enumerable, maxCollectionItems)} more items");
				break;
			}

			items.Add(ProcessObjectInternal(item, maxDepth, maxCollectionItems, currentDepth + 1));
			count++;
		}

		return items;
	}

	private static object ProcessComplexObject(object obj, int maxDepth, int maxCollectionItems, int currentDepth)
	{
		try
		{
			var properties = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
				.Where(p => p.CanRead && p.GetIndexParameters().Length == 0)
				.Take(20); // Limit to avoid too many properties

			var result = new Dictionary<string, object?>();

			foreach (var prop in properties)
			{
				try
				{
					var value = prop.GetValue(obj);
					result[prop.Name] = ProcessObjectInternal(value, maxDepth, maxCollectionItems, currentDepth + 1);
				}
				catch
				{
					result[prop.Name] = "[Error reading property]";
				}
			}

			return result;
		}
		catch
		{
			return obj.ToString();
		}
	}

	private static int GetRemainingCount(IEnumerable enumerable, int processedCount)
	{
		try
		{
			if (enumerable is ICollection collection)
				return Math.Max(0, collection.Count - processedCount);

			return enumerable.Cast<object>().Skip(processedCount).Count();
		}
		catch
		{
			return 0;
		}
	}
}