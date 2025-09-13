using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Debugging;
using SerilogLogger.Abstraction.Dtos;
using SerilogLogger.Abstraction.LoggerInterface;
using SerilogLogger.Implementation.LoggerImplementation.LogToQueue;
using SerilogLogger.Implementation.LoggerImplementation.NormalLog;
using System;
using System.IO;

namespace SerilogLogger.Implementation
{
	public static class DependencyInjection
	{
		private static readonly object _lockObject = new();
		private static bool _selfLogConfigured = false;

		public static IServiceCollection AddLoggerDependencies(this IServiceCollection services, IConfiguration baseConfig)
		{
			var config = new ConfigurationBuilder()
				.SetBasePath(AppContext.BaseDirectory)
				.AddConfiguration(baseConfig)
				.AddJsonFile("LogConfiguration.json", optional: false, reloadOnChange: true)
				.Build();

			// ✅ ساده‌سازی: خواندن مستقیم از baseConfig
			var appLogConfig = baseConfig.GetSection(ApplicationLogConfiguration.SectionName)
				.Get<ApplicationLogConfiguration>() ?? new ApplicationLogConfiguration();

			services.Configure<ApplicationLogConfiguration>(
				config.GetSection(ApplicationLogConfiguration.SectionName)
			);

			// ✅ Configure SelfLog
			ConfigureSelfLogOnce(appLogConfig);

			// ✅ Configure  default Serilog Logger
			var loggerConfig = new LoggerConfiguration()
				.MinimumLevel.Information()
				.Enrich.FromLogContext()
				.WriteTo.Console(outputTemplate: "{Timestamp:HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
				.WriteTo.File("logs/app.log",
					rollingInterval: RollingInterval.Day,
					retainedFileCountLimit: 7);

			// اگر Serilog section در config وجود داره، اون رو استفاده کن
			if (baseConfig.GetSection("Serilog").Exists())
			{
				loggerConfig.ReadFrom.Configuration(baseConfig, sectionName: "Serilog");
			}

			var logger = loggerConfig.CreateLogger();

			Log.Logger = logger;

			// ✅ Register Serilog in DI
			services.AddSingleton<Serilog.ILogger>(logger);

			// ✅ Add Microsoft Logging
			services.AddLogging(lb =>
			{
				lb.ClearProviders();

				lb.SetMinimumLevel(LogLevel.Information);

				lb.AddSerilog(logger, dispose: false);
			});

			// ✅ Register Logger Implementation بر اساس نوع
			RegisterLoggerImplementation(services, appLogConfig);

			Log.Information("Serilog configured successfully with LoggerType: {LoggerType}", appLogConfig.LoggerType);
			return services;
		}

		private static void RegisterLoggerImplementation(IServiceCollection services, ApplicationLogConfiguration config)
		{
			// ✅ انتخاب نوع Logger بر اساس کانفیگ
			if (config.LoggerType.Equals("Queue", StringComparison.OrdinalIgnoreCase))
			{
				services.AddScoped<ILog, SeriLogQueueLogger>();
			}
			else
			{
				services.AddScoped<ILog, SeriLogNormalLogger>();
			}
		}

		private static void ConfigureSelfLogOnce(ApplicationLogConfiguration appLogConfig)
		{

			if (_selfLogConfigured) return;

			lock (_lockObject)
			{
				if (_selfLogConfigured) return;

				try
				{
					if (!string.IsNullOrWhiteSpace(appLogConfig.SerilogSelfErrorLogs))
					{
						// Create directory if not exists
						var directory = Path.GetDirectoryName(appLogConfig.SerilogSelfErrorLogs);

						if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
						{
							Directory.CreateDirectory(directory);
						}

						var selfLogWriter = new StreamWriter(appLogConfig.SerilogSelfErrorLogs, append: true)
						{
							AutoFlush = true
						};

						SelfLog.Enable(selfLogWriter);
					}
					else
					{
						// اگر مسیر مشخص نشده، به console بنویس
						SelfLog.Enable(Console.Error);
					}

					_selfLogConfigured = true;
				}
				catch (Exception ex)
				{
					Console.Error.WriteLine($"[SelfLog Error] {ex.Message}");
					// Fallback to console
					SelfLog.Enable(Console.Error);
					_selfLogConfigured = true;
				}
			}
		}
	}
}