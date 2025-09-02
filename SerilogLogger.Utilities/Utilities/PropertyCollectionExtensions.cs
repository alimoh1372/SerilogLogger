using System.Collections.Generic;
using SerilogLogger.Utilities.Dtos;

namespace SerilogLogger.Utilities.Utilities;

/// <summary>
/// Collection extensions
/// </summary>
public static class PropertyCollectionExtensions
{
	public static PropertyDictionary ToPropertyDictionary(this IEnumerable<LogProperty> properties)
	{
		var dict = new PropertyDictionary();
		foreach (var prop in properties)
		{
			dict[prop.Key] = prop.Value;
		}
		return dict;
	}

	public static PropertyDictionary ToPropertyDictionary(this IEnumerable<(string Key, object? Value)> properties)
	{
		var dict = new PropertyDictionary();
		foreach (var (key, value) in properties)
		{
			dict[key] = value;
		}
		return dict;
	}
}