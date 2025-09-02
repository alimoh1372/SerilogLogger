# SerilogLogger - Advanced Serilog Wrapper

A powerful and feature-rich wrapper for Serilog that provides enhanced logging capabilities with fluent API, performance tracking, and intelligent object processing.

## ✨ Features

- ✅ **Fluent API** for easier usage
- ✅ **Automatic Performance Tracking**
- ✅ **Intelligent Object Processing**
- ✅ **Method Call Tracking**
- ✅ **Async/Queue Logging**
- ✅ **Automatic Contextual Properties**
- ✅ **Advanced Exception Handling**
- ✅ **Configurable Collection Limits**
- ✅ **Memory-efficient Large Object Handling**

## 📦 Installation

### NuGet Dependencies

```xml
<PackageReference Include="Serilog" Version="3.1.1" />
<PackageReference Include="Serilog.Sinks.Console" Version="5.0.1" />
<PackageReference Include="Serilog.Sinks.File" Version="5.0.0" />
```

### Setup DI Container

```csharp
// Program.cs or Startup.cs
using SerilogLogger.Configuration;
using SerilogLogger.Implementation.LoggerImplementation.NormalLog;
using SerilogLogger.Abstraction.LoggerInterface;

var config = new ApplicationLogConfiguration
{
    ApplicationId = "MyApp",
    ApplicationName = "My Application",
    EnablePerformanceLogging = true,
    MaxCollectionItems = 10,
    MaxObjectDepth = 5,
    EnableMethodCallTracking = false
};

var serilogLogger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("logs/app.log")
    .CreateLogger();

services.AddSingleton(config);
services.AddSingleton<Serilog.ILogger>(serilogLogger);
services.AddScoped<ILog, SeriLogNormalLogger>();
```

## ⚙️ Configuration

### ApplicationLogConfiguration

```csharp
public class ApplicationLogConfiguration
{
    // Application Information
    public string LoggerType { get; set; } = "Normal";
    public string ApplicationId { get; set; } = string.Empty;
    public string ApplicationName { get; set; } = string.Empty;
    public string SerilogSelfErrorLogs { get; set; } = string.Empty;
    
    // Performance Settings
    public bool EnablePerformanceLogging { get; set; } = true;
    public bool EnableMethodCallTracking { get; set; } = false;
    
    // Object Processing Settings
    public int MaxObjectDepth { get; set; } = 5;
    public int MaxCollectionItems { get; set; } = 10; // 0 = unlimited
    
    // Async Settings
    public bool EnableAsyncLogging { get; set; } = true;
    public int AsyncBufferSize { get; set; } = 10000;
    public TimeSpan AsyncFlushInterval { get; set; } = TimeSpan.FromSeconds(1);
    
    // Other Settings
    public bool EnableContextualProperties { get; set; } = true;
    public bool EnableExceptionDetails { get; set; } = true;
}
```

## 🚀 Quick Start

### Basic Logging

```csharp
public class UserService
{
    private readonly ILog _logger;
    
    public UserService(ILog logger)
    {
        _logger = logger;
    }
    
    public void LoginUser(string username)
    {
        // Simple logging
        _logger.Information("User logged in successfully");
        
        // With parameters
        _logger.Information("User {Username} logged in", new Dictionary<string, object?>
        {
            ["Username"] = username,
            ["LoginTime"] = DateTime.UtcNow
        });
        
        // Error logging
        try
        {
            // some operation
        }
        catch (Exception ex)
        {
            _logger.Error("Login failed for user {Username}", 
                new Dictionary<string, object?> { ["Username"] = username }, 
                ex);
        }
    }
}
```

## 🔧 PropertyDictionary

### Creation and Usage

```csharp
// Different ways to create
var props1 = new PropertyDictionary();
var props2 = PropertyDictionary.Create();
var props3 = PropertyDictionary.Create("Key", "Value");
var props4 = PropertyDictionary.From(existingDictionary);

// Fluent API
var properties = new PropertyDictionary()
    .Add("UserId", 123)
    .Add("UserName", "John")
    .Add("LoginTime", DateTime.UtcNow)
    .AddIfNotNull("Email", user.Email)
    .AddIf(user.IsAdmin, "IsAdmin", true);

_logger.Information("User activity", properties);
```

### Extension Methods

```csharp
public void PropertyDictionaryExtensions()
{
    var user = new User { Id = 1, Name = "John" };
    
    // Convert object to PropertyDictionary
    var userProps = user.ToPropertyDictionary("User");
    
    // Add object to existing PropertyDictionary
    var props = new PropertyDictionary()
        .Add("Action", "Login")
        .AddObject(user, "User")
        .AddFormatted("Message", "User {0} performed {1}", user.Name, "Login");
    
    _logger.Information("User action performed", props);
}
```

### Operators and Conversions

```csharp
public void PropertyDictionaryOperators()
{
    var props1 = new PropertyDictionary().Add("Key1", "Value1");
    var props2 = new PropertyDictionary().Add("Key2", "Value2");
    
    // Combine two PropertyDictionaries
    var combined = props1 + props2;
    
    // Implicit conversions
    PropertyDictionary fromDict = new Dictionary<string, object?> { ["Key"] = "Value" };
    Dictionary<string, object?> toDict = new PropertyDictionary().Add("Key", "Value");
    
    // Tuple conversion
    PropertyDictionary fromTuple = ("Username", "John");
    
    _logger.Information("Combined data", combined);
}
```

## 🎯 Fluent API

### Fluent Builder Pattern

```csharp
public void FluentBuilderExample()
{
    var stopwatch = Stopwatch.StartNew();
    
    try
    {
        var result = ProcessComplexOperation();
        stopwatch.Stop();
        
        _logger.StartLog()
            .Message("Complex operation completed successfully")
            .WithProperty("Duration", stopwatch.ElapsedMilliseconds)
            .WithProperty("ResultCount", result.Count)
            .WithProperty("MemoryUsed", GC.GetTotalMemory(false))
            .WithOptions(opt => {
                opt.MaxCollectionItems = 100;
                opt.EnablePerformanceLogging = true;
                opt.EnableMethodCallTracking = true;
            })
            .AsInformation();
    }
    catch (Exception ex)
    {
        stopwatch.Stop();
        
        _logger.StartLog()
            .Message("Complex operation failed")
            .WithProperty("Duration", stopwatch.ElapsedMilliseconds)
            .WithProperty("AttemptNumber", attemptCount)
            .WithException(ex)
            .WithOptions(opt => opt.MaxObjectDepth = 15)
            .AsError();
    }
}
```

### Extension Methods with PropertyDictionary

```csharp
public void FluentExtensionMethods()
{
    var props = new PropertyDictionary()
        .Add("UserId", 123)
        .Add("Action", "Purchase");
    
    // Using extension methods
    _logger.Information("User made a purchase", props);
    
    // With LoggingOptions
    _logger.Information("Large transaction", props,
        options: new LoggingOptions { EnableMethodCallTracking = true });
    
    // With Exception
    _logger.Error("Payment failed", props, exception,
        options: new LoggingOptions { MaxObjectDepth = 10 });
}
```

## ⚡ LoggingOptions

Control logging behavior for each individual log entry:

```csharp
public class LoggingOptions
{
    public bool? EnablePerformanceLogging { get; set; }
    public bool? EnableMethodCallTracking { get; set; }
    public int? MaxObjectDepth { get; set; }
    public int? MaxCollectionItems { get; set; }  // 0 = unlimited
    public bool? EnableContextualProperties { get; set; }
    public Dictionary<string, object?>? AdditionalProperties { get; set; }
}
```

### Usage Examples

```csharp
public void LoggingOptionsExamples()
{
    // For fast operations - minimal overhead
    _logger.Information("Quick operation", 
        properties,
        options: new LoggingOptions 
        { 
            EnablePerformanceLogging = false,
            EnableMethodCallTracking = false 
        });
    
    // For deep debugging
    _logger.Debug("Deep analysis", 
        properties,
        options: new LoggingOptions 
        { 
            MaxObjectDepth = 20,
            MaxCollectionItems = 0, // unlimited
            EnableMethodCallTracking = true 
        });
    
    // For critical operations
    _logger.Error("Critical error", 
        properties, 
        exception,
        options: new LoggingOptions 
        { 
            EnablePerformanceLogging = true,
            MaxObjectDepth = 15,
            AdditionalProperties = new Dictionary<string, object?>
            {
                ["ServerName"] = Environment.MachineName,
                ["ApplicationVersion"] = Assembly.GetExecutingAssembly().GetName().Version?.ToString()
            }
        });
}
```

## 📊 Performance Logging

Automatic performance tracking for logging operations:

```csharp
public void PerformanceLoggingExample()
{
    // Performance logging enabled in config
    _logger.Information("Starting heavy operation", properties);
    
    // If logging takes more than 100ms, a warning is logged:
    // "Slow logging operation: LogOperation took 150ms"
    
    // Manual control
    _logger.Information("Controlled performance tracking", 
        properties,
        options: new LoggingOptions 
        { 
            EnablePerformanceLogging = true // only for this log
        });
}
```

## 🔍 Object Processing

### Smart Collection Handling

```csharp
public void ObjectProcessingExamples()
{
    var largeList = Enumerable.Range(1, 1000).ToList();
    var complexObject = new 
    {
        Id = 1,
        Items = largeList,
        Metadata = new { Created = DateTime.Now, Tags = new[] { "tag1", "tag2" } }
    };
    
    // Default mode (MaxCollectionItems = 10)
    _logger.Information("Default processing", new Dictionary<string, object?>
    {
        ["Data"] = complexObject
    });
    // Output: Items: [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, ... and 990 more items]
    
    // Show 50 items
    _logger.Information("More items", 
        new Dictionary<string, object?> { ["Data"] = complexObject },
        null,
        new LoggingOptions { MaxCollectionItems = 50 });
    
    // Show all items (caution: may create very large logs!)
    _logger.Information("All items", 
        new Dictionary<string, object?> { ["Data"] = complexObject },
        null,
        new LoggingOptions { MaxCollectionItems = 0 });
    
    // Control object depth
    _logger.Information("Shallow processing", 
        new Dictionary<string, object?> { ["Data"] = complexObject },
        null,
        new LoggingOptions { MaxObjectDepth = 2 });
}
```

## 🚀 Queue Logger

For high-performance async logging:

```csharp
// Setup
services.AddScoped<ILog, SeriLogQueueLogger>();

public void QueueLoggerExample()
{
    // All logs are queued asynchronously
    _logger.Information("High throughput logging");
    _logger.Information("Another log");
    _logger.Information("And another");
    
    // Logs are processed in batches
    // Falls back to sync mode if buffer is full
}
```

## 📝 Real-World Examples

### Web API Controller

```csharp
[ApiController]
public class UsersController : ControllerBase
{
    private readonly ILog _logger;
    private readonly IUserService _userService;
    
    public UsersController(ILog logger, IUserService userService)
    {
        _logger = logger;
        _userService = userService;
    }
    
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var requestId = Guid.NewGuid().ToString();
        
        _logger.StartLog()
            .Message("Login request received")
            .WithProperty("RequestId", requestId)
            .WithProperty("Username", request.Username)
            .WithProperty("IPAddress", Request.HttpContext.Connection.RemoteIpAddress?.ToString())
            .WithOptions(opt => opt.EnableMethodCallTracking = true)
            .AsInformation();
        
        try
        {
            var result = await _userService.LoginAsync(request);
            
            _logger.Information("Login successful", 
                new PropertyDictionary()
                    .Add("RequestId", requestId)
                    .Add("UserId", result.UserId)
                    .Add("Username", request.Username),
                options: new LoggingOptions { EnablePerformanceLogging = true });
            
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.Warning("Login failed - invalid credentials", 
                new PropertyDictionary()
                    .Add("RequestId", requestId)
                    .Add("Username", request.Username)
                    .Add("Reason", "InvalidCredentials"), 
                ex);
            
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.Error("Login failed - system error", 
                new PropertyDictionary()
                    .Add("RequestId", requestId)
                    .Add("Username", request.Username), 
                ex,
                options: new LoggingOptions 
                { 
                    MaxObjectDepth = 10,
                    EnableMethodCallTracking = true 
                });
            
            return StatusCode(500);
        }
    }
}
```

### Repository Pattern

```csharp
public class UserRepository : IUserRepository
{
    private readonly ILog _logger;
    private readonly DbContext _context;
    
    public async Task<User?> GetByIdAsync(int userId)
    {
        _logger.Debug("Fetching user by ID", 
            FluentILogExtensions.Properties("UserId", userId));
        
        try
        {
            var user = await _context.Users
                .Include(u => u.Orders)
                .FirstOrDefaultAsync(u => u.Id == userId);
            
            if (user == null)
            {
                _logger.Warning("User not found", 
                    new PropertyDictionary().Add("UserId", userId));
                return null;
            }
            
            _logger.Debug("User fetched successfully", 
                user.ToPropertyDictionary("User"),
                options: new LoggingOptions { MaxCollectionItems = 5 });
            
            return user;
        }
        catch (Exception ex)
        {
            _logger.Error("Failed to fetch user", 
                new PropertyDictionary().Add("UserId", userId), 
                ex);
            throw;
        }
    }
}
```

### Background Service

```csharp
public class DataProcessingService : BackgroundService
{
    private readonly ILog _logger;
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var batchId = Guid.NewGuid().ToString();
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                var items = await GetPendingItemsAsync();
                
                _logger.Information("Starting batch processing", 
                    new PropertyDictionary()
                        .Add("BatchId", batchId)
                        .Add("ItemCount", items.Count)
                        .Add("StartTime", DateTime.UtcNow));
                
                var results = await ProcessItemsAsync(items);
                stopwatch.Stop();
                
                _logger.StartLog()
                    .Message("Batch processing completed")
                    .WithProperty("BatchId", batchId)
                    .WithProperty("ItemCount", items.Count)
                    .WithProperty("SuccessCount", results.SuccessCount)
                    .WithProperty("FailureCount", results.FailureCount)
                    .WithProperty("Duration", stopwatch.ElapsedMilliseconds)
                    .WithProperty("Results", results.Details)
                    .WithOptions(opt => {
                        opt.MaxCollectionItems = 20;
                        opt.EnablePerformanceLogging = true;
                    })
                    .AsInformation();
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                
                _logger.Error("Batch processing failed", 
                    new PropertyDictionary()
                        .Add("BatchId", batchId)
                        .Add("Duration", stopwatch.ElapsedMilliseconds), 
                    ex,
                    options: new LoggingOptions { MaxObjectDepth = 15 });
            }
            
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}
```

## ✅ Best Practices

### 1. Performance

```csharp
// ✅ Good - Use structured logging
_logger.Information("User {UserId} completed {Action}", 
    new Dictionary<string, object?>
    {
        ["UserId"] = userId,
        ["Action"] = action
    });

// ❌ Bad - String concatenation
_logger.Information($"User {userId} completed {action}");
```

### 2. Exception Handling

```csharp
// ✅ Good
try
{
    // operation
}
catch (Exception ex)
{
    _logger.Error("Operation failed for user {UserId}", 
        new Dictionary<string, object?> { ["UserId"] = userId },
        ex); // Pass exception separately
    throw;
}
```

### 3. Large Objects

```csharp
// ✅ Good - Control MaxCollectionItems
_logger.Information("Processing batch", 
    new Dictionary<string, object?> { ["Items"] = largeCollection },
    null,
    new LoggingOptions { MaxCollectionItems = 5 });

// ❌ Bad - No control (may create huge logs)
_logger.Information("Processing batch", 
    new Dictionary<string, object?> { ["Items"] = largeCollection });
```

### 4. Conditional Logging

```csharp
// ✅ Good - Use AddIf methods
var props = new PropertyDictionary()
    .Add("UserId", userId)
    .AddIf(includeDetails, "Details", detailsObject)
    .AddIfNotNull("OptionalData", optionalData);

_logger.Information("User action", props);
```

## 📚 API Reference

### Core Interfaces

```csharp
public interface ILog
{
    void Debug(string message, Dictionary<string, object?>? properties = null, Exception? exception = null, LoggingOptions? options = null);
    void Information(string message, Dictionary<string, object?>? properties = null, Exception? exception = null, LoggingOptions? options = null);
    void Warning(string message, Dictionary<string, object?>? properties = null, Exception? exception = null, LoggingOptions? options = null);
    void Error(string message, Dictionary<string, object?>? properties = null, Exception? exception = null, LoggingOptions? options = null);
    void Fatal(string message, Dictionary<string, object?>? properties = null, Exception? exception = null, LoggingOptions? options = null);
}
```

### Extension Methods

```csharp
// PropertyDictionary extensions
public static PropertyDictionary ToPropertyDictionary<T>(this T obj, string? prefix = null)
public static PropertyDictionary AddObject<T>(this PropertyDictionary properties, T obj, string prefix)
public static PropertyDictionary AddFormatted(this PropertyDictionary properties, string key, string format, params object[] args)

// ILog extensions  
public static void Information(this ILog logger, string message, PropertyDictionary properties, LoggingOptions? options = null)
public static void Error(this ILog logger, string message, PropertyDictionary properties, Exception exception, LoggingOptions? options = null)
public static ILogBuilder StartLog(this ILog logger)

// Helper methods
public static PropertyDictionary Properties(string key, object? value)
public static PropertyDictionary Properties()
```

## 🔧 Configuration Options

### Logger Types

- **SeriLogNormalLogger**: Standard synchronous logging
- **SeriLogQueueLogger**: High-performance asynchronous logging with queue

### Performance Tuning

- Set `MaxCollectionItems = 0` for unlimited collection logging (use with caution)
- Use `MaxObjectDepth` to control serialization depth
- Enable `EnablePerformanceLogging` for slow operation detection
- Use `SeriLogQueueLogger` for high-throughput scenarios

### Memory Management

- Large collections are automatically truncated based on `MaxCollectionItems`
- Deep object graphs are limited by `MaxObjectDepth`
- Async logging reduces memory pressure on main thread

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🔗 Related Projects

- [Serilog](https://serilog.net/) - The underlying logging library
- [Serilog.Extensions.Logging](https://github.com/serilog/serilog-extensions-logging) - Microsoft.Extensions.Logging integration

---

**Made with ❤️ for better logging experiences**