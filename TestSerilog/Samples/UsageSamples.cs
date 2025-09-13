using SerilogLogger.Abstraction.LoggerInterface;
using SerilogLogger.Utilities.Utilities;

namespace TestSerilog.Samples;

public class UsageSamples
{
	private readonly ILog _logger;

	public UsageSamples(ILog logger)
	{
		_logger = logger;
	}

	public void DemonstrateUsage()
	{
		var user = new { Id = 123, Name = "احمد رضایی", Email = "ahmad@example.com" };

		// ✅ حل شده - استفاده از WithObject برای object
		_logger.Information("User profile updated",
			user.ToPropertyDictionary("User")
				.With("UpdateTime", DateTime.Now)
				.With("UpdatedBy", "System"));

		_logger.Debug("Database query executed",
			LogPropertyBuilder.New("QueryType", "SELECT")
				.With("TableName", "Users")
				.With("Duration", 125)
				.WithObject(user, "Result")
			);

		_logger.Information("API call completed",
			LogPropertyBuilder.From(user, "ResponseData")
				.With("StatusCode", 200)
				.With("ResponseTime", 85));

		var props = new[]
		{
			("EventType", (object?)"UserLogin"),
			("Timestamp", DateTime.Now),
			("Success", true)
		}.ToPropertyDictionary();

		_logger.Information("Login attempt", props);

		// نمونه‌های اضافی
		DemonstrateOtherMethods(user);
	}

	private void DemonstrateOtherMethods(object user)
	{
		// استفاده از LogPropertyBuilder.New() خالی - حل شده
		_logger.Information("Simple log",
			LogPropertyBuilder.New()
				.With("Action", "Login")
				.With("Success", true));

		// ترکیب PropertyDictionary با tuple
		var additionalProps = new[]
		{
			("Source", (object?)"Mobile"),
			("Version", "1.2.3")
		}.ToPropertyDictionary();

		_logger.Warning("Version mismatch detected", additionalProps);

		// استفاده از Props() helper - حل مشکل ابهام
		_logger.Error("Database connection failed",
			FluentLoggerExtensions.Props("ConnectionString", "***hidden***")
				.With("RetryCount", 3)
				.With("LastError", "Timeout"));

		// استفاده از WithProps برای anonymous objects
		_logger.Information("Complex operation completed",
			FluentLoggerExtensions.Props()
				.WithProps(new { Operation = "DataSync", Duration = 1500 })
				.With("Success", true));
	}
}