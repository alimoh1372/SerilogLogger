using SerilogLogger.Utilities.Utilities;

namespace TestSerilog.Models;

public class SensitiveUser
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    [LogIgnore]
    public string Password { get; set; } = string.Empty;

    [LogIgnore]
    public string CreditCard { get; set; } = string.Empty;
}