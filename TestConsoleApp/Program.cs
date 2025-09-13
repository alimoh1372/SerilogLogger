#region PerformanceTest

using TestConsoleApp.PerformanceTest;

Console.WriteLine("🏁 Professional Logger Performance Test Suite");
Console.WriteLine("==============================================");

var testSuite = new PerformanceTestSuite();

try
{
	testSuite.Setup();

	// Run all test categories
	testSuite.RunSingleThreadedTests();
	testSuite.RunMultiThreadedTests();
	testSuite.RunStressTests();

	// Final summary
	testSuite.PrintSummary();

	testSuite.Cleanup();

	Console.WriteLine("\n🎯 All performance tests completed!");
	Console.WriteLine("📝 Results show your logger's true performance capabilities.");
	Console.WriteLine("\nPress any key to exit...");
	Console.ReadKey();
}
catch (Exception ex)
{
	Console.WriteLine($"\n❌ Performance test failed: {ex.Message}");
	Console.WriteLine($"Stack trace: {ex.StackTrace}");
}


#endregion


#region SimpleMemoryTest

//using TestConsoleApp.SimpleTest;

//Console.WriteLine("🧪 Simple Memory Test (No BenchmarkDotNet)");
//Console.WriteLine("===========================================");

//var test = new SimpleMemoryTest();

//try
//{
//	test.Setup();

//	// تست های مختلف
//	test.TestSimpleMessages();
//	test.TestWithExceptions();
//	test.CompareAllocationStrategies();
//	test.SimulateBenchmarkTests(); // ✅ تست جدید اضافه شد
//	test.LongRunningLeakTest();

//	test.Cleanup();

//	Console.WriteLine("\n✅ All tests completed successfully!");
//	Console.WriteLine("\nPress any key to exit...");
//	Console.ReadKey();
//}
//catch (Exception ex)
//{
//	Console.WriteLine($"\n❌ Error: {ex.Message}");
//	Console.WriteLine($"Stack: {ex.StackTrace}");
//}


#endregion

#region Benchmark Test

//using BenchmarkDotNet.Running;
//using TestConsoleApp.BenchMarkTest;

//BenchmarkRunner.Run<ThroughputAllocationOptimizedTest>();

//Console.ReadKey();
#endregion
#region SimpleTest
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;
//using SerilogLogger.Implementation;
//using SerilogLogger.Abstraction.LoggerInterface;
//using Serilog;

//Serilog.Debugging.SelfLog.Enable(msg => Console.Error.WriteLine("SERILOG-SELFLOG: " + msg));

//// ➊ لود تنظیمات با BasePath مطمئن
//var configuration = new ConfigurationBuilder()
//	.SetBasePath(AppContext.BaseDirectory)
//	.AddJsonFile("LogConfiguration.json", optional: false, reloadOnChange: true)
//	.Build();

//// ➋ چاپ یه کلید برای اطمینان از لود شدن فایل
//Console.WriteLine("Config Level: " + (configuration["Serilog:MinimumLevel:Default"] ?? "(null)"));

//// ➌ DI و لاگر
//var services = new ServiceCollection()
//	.AddLoggerDependencies(configuration)
//	.BuildServiceProvider();

//var logger = services.GetRequiredService<ILog>();

//// ➍ تست لاگ‌ها
//logger.Information("Starting application...",
//	new Dictionary<string, object?> { ["UserId"] = 123, ["Action"] = "Login" });

//logger.Warning("Low disk space detected", new Dictionary<string, object?> { ["Drive"] = "C:", ["FreeSpaceGB"] = 1.5 });

//try { throw new InvalidOperationException("Something went wrong!"); }
//catch (Exception ex) { logger.Error("Unhandled exception occurred", exception: ex); }

//Console.WriteLine("✅ Logging test finished. Check console and log files.");
//Console.ReadKey();


#endregion
