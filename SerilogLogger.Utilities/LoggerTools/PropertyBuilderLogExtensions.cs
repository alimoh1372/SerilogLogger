using SerilogLogger.Abstraction.LoggerInterface;
using SerilogLogger.Utilities.Dtos;
using System.Runtime.CompilerServices;

namespace SerilogLogger.Utilities.LoggerTools;



/// <summary>
/// Extension methods برای ILog با Property Builder
/// </summary>
public static class PropertyBuilderLogExtensions
{
	public static void Information(this ILog logger, string messageTemplate,
		Func<PropertyDictionary, PropertyDictionary> propertyBuilder,
	[CallerMemberName] string callerName = null!,
	[CallerFilePath] string callerPath = null!)
	{
		var props = propertyBuilder(new PropertyDictionary());
		logger.Information(messageTemplate, props.ToDictionary(),null,null ,callerName, callerPath);
	}

	public static void Debug(this ILog logger, string messageTemplate,
		Func<PropertyDictionary, PropertyDictionary> propertyBuilder,
	[CallerMemberName] string callerName = null!,
	[CallerFilePath] string callerPath = null!)
	{
		var props = propertyBuilder(new PropertyDictionary());
		logger.Debug(messageTemplate, props.ToDictionary(), null, null, callerName, callerPath);
	}

	public static void Warning(this ILog logger, string messageTemplate,
		Func<PropertyDictionary, PropertyDictionary> propertyBuilder,
		[CallerMemberName] string callerName = null!,
		[CallerFilePath] string callerPath = null!)
	{
		var props = propertyBuilder(new PropertyDictionary());
		logger.Warning(messageTemplate, props.ToDictionary(), null, null, callerName, callerPath);
	}

	public static void Error(this ILog logger, string messageTemplate,
		Func<PropertyDictionary, PropertyDictionary> propertyBuilder, 
		[CallerMemberName] string callerName = null!,
		[CallerFilePath] string callerPath = null!,
		Exception? exception = null)
	{
		var props = propertyBuilder(new PropertyDictionary());
		logger.Error(messageTemplate, props.ToDictionary(), null, null, callerName, callerPath);
	}
}
