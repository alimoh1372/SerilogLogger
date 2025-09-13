// ===================================
// LoggingOptions.cs - Per-call options
// ===================================
namespace SerilogLogger.Abstraction.Dtos;

public class LoggingOptions
{
	public bool? EnablePerformanceLogging { get; set; }
	public bool? EnableMethodCallTracking { get; set; }
	public int? MaxObjectDepth { get; set; }
	public int? MaxCollectionItems { get; set; }
	public bool? EnableContextualProperties { get; set; }
	public Dictionary<string, object?>? AdditionalProperties { get; set; }
}