namespace TestSerilog.Models;

public class UserProfile
{
    public int Age { get; set; }
    public string City { get; set; } = string.Empty;
    public List<string> Interests { get; set; } = new();
}