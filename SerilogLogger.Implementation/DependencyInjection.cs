using Microsoft.Extensions.DependencyInjection;
using SerilogLogger.Abstraction.Dtos;
using SerilogLogger.Abstraction.LoggerInterface;
using SerilogLogger.Implementation.LoggerImplementation.LogToQueue;
using SerilogLogger.Implementation.LoggerImplementation.NormalLog;

namespace SerilogLogger.Implementation;

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