using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SerilogLogger.Abstraction.LoggerInterface;
using SerilogLogger.Implementation;
using System;
using System.Collections.Generic;

namespace TestConsoleApp.BenchMarkTest;

[MemoryDiagnoser]
[ThreadingDiagnoser]
[SimpleJob] // ✅ ساده‌ترین راه - بدون config اضافی
public class ThroughputAllocationOptimizedTest
{
	private ILog _logger = null!;
	private string _bigMessage = string.Empty;
	private Exception _smallException = null!;
	private Exception _bigException = null!;

	// ✅ Object pooling برای Dictionary ها
	private readonly Dictionary<string, object?> _smallDict = new(1);
	private readonly Dictionary<string, object?> _mediumDict = new(3);
	private readonly Dictionary<string, object?> _largeDict = new(1);
	private readonly Dictionary<string, object?> _complexDict = new(1);
	private readonly Dictionary<string, object?> _exceptionDict = new(1);

	[GlobalSetup]
	public void Setup()
	{
		var configuration = new ConfigurationBuilder()
			.AddJsonFile("LogConfiguration.json", optional: false, reloadOnChange: true)
			.Build();

		var services = new ServiceCollection();
		services.AddLoggerDependencies(configuration);

		var provider = services.BuildServiceProvider();
		_logger = provider.GetRequiredService<ILog>();

		_bigMessage = new string('X', 10000);
		_smallException = new InvalidOperationException("Something went wrong!");

		_bigException = new InvalidOperationException("Outer exception occurred",
			new ArgumentNullException("paramName", "Inner exception details"))
		{
			HelpLink = "https://example.com/error",
			Source = "BenchmarkTest"
		};

		_bigException.Data["CorrelationId"] = Guid.NewGuid().ToString();
		_bigException.Data["UserId"] = 1234;
		_bigException.Data["Payload"] = new string('Y', 5000);
	}

	[GlobalCleanup]
	public void Cleanup()
	{
		_smallDict.Clear();
		_mediumDict.Clear();
		_largeDict.Clear();
		_complexDict.Clear();
		_exceptionDict.Clear();

		// ✅ Force GC cleanup
		GC.Collect();
		GC.WaitForPendingFinalizers();
		GC.Collect();
	}

	// ✅ بهبود: استفاده مجدد از Dictionary
	[Benchmark]
	public void Log1000SmallMessages()
	{
		for (int i = 0; i < 1000; i++)
		{
			_smallDict.Clear();
			_smallDict["i"] = i;
			_logger.Information("Hello World {i}", _smallDict);
		}
	}

	[Benchmark]
	public void Log1000MediumMessages()
	{
		for (int i = 0; i < 1000; i++)
		{
			_mediumDict.Clear();
			_mediumDict["UserId"] = 1000 + i;
			_mediumDict["Action"] = "Login"; // ✅ String interning
			_mediumDict["Index"] = i;

			_logger.Information("Medium message {UserId} {Action} {Index}", _mediumDict);
		}
	}

	[Benchmark]
	public void Log1000LargeMessages()
	{
		for (int i = 0; i < 1000; i++)
		{
			_largeDict.Clear();
			_largeDict["BigText"] = _bigMessage; // ✅ استفاده مجدد از string

			_logger.Error("Large payload {BigText}", _largeDict);
		}
	}

	[Benchmark]
	public void Log1000SmallExceptions()
	{
		for (int i = 0; i < 1000; i++)
		{
			_exceptionDict.Clear();
			_exceptionDict["Index"] = i;

			_logger.Error("Error occurred {Index}", _exceptionDict, _smallException);
		}
	}

	// ✅ مقایسه با روش قدیمی
	[Benchmark(Baseline = true)]
	public void Log1000SmallMessages_OldWay()
	{
		for (int i = 0; i < 1000; i++)
		{
			_logger.Information("Hello World {i}",
				new Dictionary<string, object?> { ["i"] = i }); // هر بار new Dictionary
		}
	}
}