using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using SerilogLogger.LoggerInterface;
using SerilogLogger.Utilities;

namespace TestSerilog.Controllers;

public class LogSample
{
    private readonly ILog _logger;

    private readonly string _className;

    public LogSample(ILog logger)
    {
        _className = this.GetType().Name;

        _logger = logger;
    }

    public void LogSampleMethode()
    {
        var methodName = MethodName.GetName();
        
        try
        {
            _logger.Verbose("Verbose message",
                new List<KeyValuePair<string, object>>()
                {
                    new("ClassName", _className),
                    new("MethodName", methodName)
                    // other parameters
                });

            _logger.Debug("Debug message",
                new List<KeyValuePair<string, object>>()
                {
                    new("ClassName", _className),
                    new("MethodName", methodName)
                    // other parameters
                });

            _logger.Information("Information message",
                new List<KeyValuePair<string, object>>()
                {
                    new("ClassName", _className),
                    new("MethodName", methodName)
                    // other parameters
                });

            _logger.Warning("Warning message",
                new List<KeyValuePair<string, object>>()
                {
                    new("ClassName", _className),
                    new("MethodName", methodName)
                    // other parameters
                });

            _logger.Fatal("Fatal message",
                new List<KeyValuePair<string, object>>()
                {
                    new("ClassName", _className),
                    new("MethodName", methodName)
                    // other parameters
                });

            

            // pass dto to logger and logger unpack dto for logging with `UnionKeyValuePairs` 

            var obj =new
            {
                Username = "user",
                Password = "password"
            };

            _logger.Information("Information message",
                new List<KeyValuePair<string, object>>()
                {
                    new("ClassName", _className),
                    new("MethodName", methodName)
                    // other parameters
                }.UnionKeyValuePairs(obj));

        }
        catch (Exception ex)
        {
            // pass exception to logger
            _logger.Error("error message",
                new List<KeyValuePair<string, object>>()
                {
                    new("ClassName", _className),
                    new("MethodName", methodName)
                }, ex);
        }
    }
}