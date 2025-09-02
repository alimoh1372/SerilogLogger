namespace SerilogLogger.Abstraction.Dtos;

public class ApplicationLogConfiguration
{
	// ✅ اضافه کردن SectionName constant
	public const string SectionName = "LogConfiguration";

	public string LoggerType { get; set; } = "Normal";
	public string SerilogSelfErrorLogs { get; set; } = "C:\\Logs\\SerilogSelfErrors\\";

	// Enhanced Configuration Properties
	public bool EnablePerformanceLogging { get; set; } = true;
	public bool EnableMethodCallTracking { get; set; } = false;
	public int MaxObjectDepth { get; set; } = 5;
	public int MaxCollectionItems { get; set; } = 10; // 0 = unlimited
	public bool EnableAsyncLogging { get; set; } = true;
	public int AsyncBufferSize { get; set; } = 10000;
	public TimeSpan AsyncFlushInterval { get; set; } = TimeSpan.FromSeconds(1);
	public bool EnableContextualProperties { get; set; } = true;
	public bool EnableExceptionDetails { get; set; } = true;
}