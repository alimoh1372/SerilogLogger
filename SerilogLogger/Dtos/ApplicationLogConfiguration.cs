namespace SerilogLogger.Dtos;

public class ApplicationLogConfiguration
{
    public const string ConfigurationSectionName = "ApplicationLogConfiguration";

    public string ApplicationId { get; set; } = null!;

    public string ApplicationName { get; set; } = null!;

    public bool IsLogToQueue { get; set; }
}