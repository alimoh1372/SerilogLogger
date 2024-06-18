using Microsoft.Extensions.DependencyInjection;
using SerilogLogger.Dtos;
using SerilogLogger.LoggerImplementation.LogToQueue;
using SerilogLogger.LoggerImplementation.NormalLog;
using SerilogLogger.LoggerInterface;

namespace SerilogLogger;

public static class DependencyInjection
{
    public static IServiceCollection AddLoggerDependencies(this IServiceCollection services, ApplicationLogConfiguration logConfiguration)
    {
        if (logConfiguration.IsLogToQueue)
            services.AddSingleton<ILog, SeriLogQueueLogger>(_=> new SeriLogQueueLogger(logConfiguration.ApplicationId,logConfiguration.ApplicationName));
        else
            services.AddSingleton<ILog, SeriLogNormalLogger>(_ => new SeriLogNormalLogger(logConfiguration.ApplicationId, logConfiguration.ApplicationName));
        
        return services;
    }
}