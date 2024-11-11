# steps to setup logger:

## 1. add `SerilogLogger.csproj` refrence to sulotion


## 2. create `LogConfiguration.json` for log setting


## 3. add following configuration to `appsetting.json`

```
"ApplicationLogConfiguration": {

  "ApplicationId": "15",

  "ApplicationName": "Ressa",

  "IsLogToQueue": true
}
```

4. add log service to di

```
var applicationConfiguration = configuration
    .GetSection(nameof(ApplicationLogConfiguration))
    .Get<ApplicationLogConfiguration>()!;

services.AddLoggerDependencies(applicationConfiguration);
```


### if `IsLogToQueue` is set to True all logging will be doen in single thread and reduce io time thus increasing performance 



# useage:

```
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
```



#### recomended log properties (like above sample)

1. class name
2. method name 


#### defult enrich in all logs (defult log properties)

1. Machine Name
2. Thread Id
3. Thread Name
4. Process Name
5. Process Id
6. Host Name
7. User Name
8. Application Id (read from `ApplicationLogConfiguration` of appsetting)
9. Application Name (read from `ApplicationLogConfiguration` of appsetting)
10. Domain (the "Domain" Enrich is for application monitor)
