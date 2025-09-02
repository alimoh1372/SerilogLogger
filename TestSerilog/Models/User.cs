namespace TestSerilog.Models;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserProfile? Profile { get; set; }
    public List<Order> Orders { get; set; } = new();
}