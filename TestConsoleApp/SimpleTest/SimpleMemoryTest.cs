using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SerilogLogger.Abstraction.LoggerInterface;
using SerilogLogger.Implementation;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TestConsoleApp.SimpleTest
{
	public class SimpleMemoryTest
	{
		private ILog _logger = null!;
		private readonly Dictionary<string, object?> _reusableDict = new(4);
		private Exception _testException = null!;

		public void Setup()
		{
			var configuration = new ConfigurationBuilder()
				.AddJsonFile("LogConfiguration.json", optional: false, reloadOnChange: true)
				.Build();

			var services = new ServiceCollection();
			services.AddLoggerDependencies(configuration);

			var provider = services.BuildServiceProvider();
			_logger = provider.GetRequiredService<ILog>();

			_testException = new InvalidOperationException("Test exception");
		}

		// تست جدید: شبیه‌سازی Benchmark اصلی
		public void SimulateBenchmarkTests()
		{
			Console.WriteLine("\n🔄 Simulating Original Benchmark Tests...");

			// تست با پیام‌های مختلف
			TestBigMessages();
			TestComplexStructuredLogs();
			TestMixedScenario();
		}

		private void TestBigMessages()
		{
			Console.WriteLine("\n--- Big Messages Test (5,000 messages) ---");
			var sw = Stopwatch.StartNew();
			var initialMemory = GC.GetTotalMemory(false);

			var bigMessage = new string('X', 10000); // 10KB message

			for (int i = 0; i < 5000; i++)
			{
				_reusableDict.Clear();
				_reusableDict["BigText"] = bigMessage;
				_reusableDict["Index"] = i;
				_logger.Error("Large payload {BigText} at {Index}", _reusableDict);

				if (i % 500 == 0)
				{
					var currentMemory = GC.GetTotalMemory(false);
					Console.WriteLine($"  Big messages: {i:N0}, Memory: {(currentMemory - initialMemory) / 1024:N0} KB");
				}
			}

			var beforeGC = GC.GetTotalMemory(false);
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
			var afterGC = GC.GetTotalMemory(false);

			sw.Stop();
			Console.WriteLine($"⏱️  Time: {sw.ElapsedMilliseconds} ms");
			Console.WriteLine($"💾 Memory used: {(beforeGC - initialMemory) / 1024:N0} KB");
			Console.WriteLine($"🗑️  After GC: {(afterGC - initialMemory) / 1024:N0} KB");
		}

		private void TestComplexStructuredLogs()
		{
			Console.WriteLine("\n--- Complex Structured Logs Test (3,000 logs) ---");
			var sw = Stopwatch.StartNew();
			var initialMemory = GC.GetTotalMemory(false);

			var complexPayload = new string('Y', 5000); // 5KB payload

			for (int i = 0; i < 3000; i++)
			{
				_reusableDict.Clear();
				_reusableDict["UserId"] = 1000 + i;
				_reusableDict["Action"] = "ComplexOperation";
				_reusableDict["Payload"] = $"{complexPayload}-{i}";
				_reusableDict["Timestamp"] = DateTime.Now;
				_reusableDict["SessionId"] = Guid.NewGuid();

				_logger.Information("Complex structured log {UserId} {Action} {Payload} {Timestamp} {SessionId}", _reusableDict);

				if (i % 300 == 0)
				{
					var currentMemory = GC.GetTotalMemory(false);
					Console.WriteLine($"  Complex logs: {i:N0}, Memory: {(currentMemory - initialMemory) / 1024:N0} KB");
				}
			}

			var beforeGC = GC.GetTotalMemory(false);
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
			var afterGC = GC.GetTotalMemory(false);

			sw.Stop();
			Console.WriteLine($"⏱️  Time: {sw.ElapsedMilliseconds} ms");
			Console.WriteLine($"💾 Memory used: {(beforeGC - initialMemory) / 1024:N0} KB");
			Console.WriteLine($"🗑️  After GC: {(afterGC - initialMemory) / 1024:N0} KB");
		}

		private void TestMixedScenario()
		{
			Console.WriteLine("\n--- Mixed Scenario Test (10,000 mixed logs) ---");
			var sw = Stopwatch.StartNew();
			var initialMemory = GC.GetTotalMemory(false);

			for (int i = 0; i < 10000; i++)
			{
				_reusableDict.Clear();

				// مخلوطی از انواع مختلف log مثل benchmark اصلی
				switch (i % 5)
				{
					case 0: // Small message
						_reusableDict["i"] = i;
						_logger.Information("Hello World {i}", _reusableDict);
						break;

					case 1: // Medium message
						_reusableDict["UserId"] = 1000 + i;
						_reusableDict["Action"] = "Login";
						_reusableDict["Index"] = i;
						_logger.Information("Medium message {UserId} {Action} {Index}", _reusableDict);
						break;

					case 2: // Large message
						_reusableDict["BigText"] = new string('X', 1000); // 1KB
						_logger.Error("Large payload {BigText}", _reusableDict);
						break;

					case 3: // Exception
						_reusableDict["Index"] = i;
						_logger.Error("Error occurred {Index}", _reusableDict, _testException);
						break;

					case 4: // Complex structured
						_reusableDict["Payload"] = new string('Z', 500) + $"-{i}";
						_logger.Information("Structured log with payload {Payload}", _reusableDict);
						break;
				}

				if (i % 1000 == 0)
				{
					var currentMemory = GC.GetTotalMemory(false);
					Console.WriteLine($"  Mixed logs: {i:N0}, Memory: {(currentMemory - initialMemory) / 1024:N0} KB");
				}
			}

			var beforeGC = GC.GetTotalMemory(false);
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
			var afterGC = GC.GetTotalMemory(false);

			sw.Stop();
			Console.WriteLine($"⏱️  Time: {sw.ElapsedMilliseconds} ms");
			Console.WriteLine($"💾 Memory used: {(beforeGC - initialMemory) / 1024:N0} KB");
			Console.WriteLine($"🗑️  After GC: {(afterGC - initialMemory) / 1024:N0} KB");
		}

		// تست ساده - 50,000 پیام
		public void TestSimpleMessages()
		{
			Console.WriteLine("🔄 Starting Simple Messages Test (50,000 messages)...");
			var sw = Stopwatch.StartNew();
			var initialMemory = GC.GetTotalMemory(false);

			for (int i = 0; i < 50000; i++)
			{
				_reusableDict.Clear();
				_reusableDict["Index"] = i;
				_logger.Information("Simple message {Index}", _reusableDict);

				// هر 5000 پیام memory چک کنیم
				if (i % 5000 == 0)
				{
					var currentMemory = GC.GetTotalMemory(false);
					Console.WriteLine($"Messages: {i:N0}, Memory: {(currentMemory - initialMemory) / 1024:N0} KB");
				}
			}

			sw.Stop();

			// Force GC و memory check
			var beforeGC = GC.GetTotalMemory(false);
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
			var afterGC = GC.GetTotalMemory(false);

			Console.WriteLine($"✅ Test completed in {sw.ElapsedMilliseconds} ms");
			Console.WriteLine($"📊 Memory before GC: {(beforeGC - initialMemory) / 1024:N0} KB");
			Console.WriteLine($"📊 Memory after GC: {(afterGC - initialMemory) / 1024:N0} KB");
			Console.WriteLine($"🗑️  GC freed: {(beforeGC - afterGC) / 1024:N0} KB");
		}

		// تست با Exception - 10,000
		public void TestWithExceptions()
		{
			Console.WriteLine("\n🔄 Starting Exception Test (10,000 exceptions)...");
			var sw = Stopwatch.StartNew();
			var initialMemory = GC.GetTotalMemory(false);

			for (int i = 0; i < 10000; i++)
			{
				_reusableDict.Clear();
				_reusableDict["Index"] = i;
				_logger.Error("Error occurred {Index}", _reusableDict, _testException);

				if (i % 1000 == 0)
				{
					var currentMemory = GC.GetTotalMemory(false);
					Console.WriteLine($"Exceptions: {i:N0}, Memory: {(currentMemory - initialMemory) / 1024:N0} KB");
				}
			}

			sw.Stop();

			var beforeGC = GC.GetTotalMemory(false);
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
			var afterGC = GC.GetTotalMemory(false);

			Console.WriteLine($"✅ Exception test completed in {sw.ElapsedMilliseconds} ms");
			Console.WriteLine($"📊 Memory before GC: {(beforeGC - initialMemory) / 1024:N0} KB");
			Console.WriteLine($"📊 Memory after GC: {(afterGC - initialMemory) / 1024:N0} KB");
			Console.WriteLine($"🗑️  GC freed: {(beforeGC - afterGC) / 1024:N0} KB");
		}

		// تست مقایسه: New Dictionary vs Reuse - 25,000 هر کدام
		public void CompareAllocationStrategies()
		{
			Console.WriteLine("\n🔄 Comparing Allocation Strategies (25,000 each)...");

			// Test 1: New Dictionary هر بار (مثل کد قدیمی)
			Console.WriteLine("\n--- Test 1: New Dictionary Every Time ---");
			var sw1 = Stopwatch.StartNew();
			var mem1_initial = GC.GetTotalMemory(false);

			for (int i = 0; i < 25000; i++)
			{
				var newDict = new Dictionary<string, object?> { ["Index"] = i };
				_logger.Information("New dict message {Index}", newDict);

				if (i % 5000 == 0)
				{
					var currentMemory = GC.GetTotalMemory(false);
					Console.WriteLine($"  Progress: {i:N0}, Memory: {(currentMemory - mem1_initial) / 1024:N0} KB");
				}
			}

			var mem1_beforeGC = GC.GetTotalMemory(false);
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
			var mem1_afterGC = GC.GetTotalMemory(false);
			sw1.Stop();

			Console.WriteLine($"⏱️  Time: {sw1.ElapsedMilliseconds} ms");
			Console.WriteLine($"💾 Memory used: {(mem1_beforeGC - mem1_initial) / 1024:N0} KB");
			Console.WriteLine($"🗑️  After GC: {(mem1_afterGC - mem1_initial) / 1024:N0} KB");

			// Test 2: Reuse Dictionary (کد جدید)
			Console.WriteLine("\n--- Test 2: Reuse Dictionary ---");
			var sw2 = Stopwatch.StartNew();
			var mem2_initial = GC.GetTotalMemory(false);

			for (int i = 0; i < 25000; i++)
			{
				_reusableDict.Clear();
				_reusableDict["Index"] = i;
				_logger.Information("Reused dict message {Index}", _reusableDict);

				if (i % 5000 == 0)
				{
					var currentMemory = GC.GetTotalMemory(false);
					Console.WriteLine($"  Progress: {i:N0}, Memory: {(currentMemory - mem2_initial) / 1024:N0} KB");
				}
			}

			var mem2_beforeGC = GC.GetTotalMemory(false);
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
			var mem2_afterGC = GC.GetTotalMemory(false);
			sw2.Stop();

			Console.WriteLine($"⏱️  Time: {sw2.ElapsedMilliseconds} ms");
			Console.WriteLine($"💾 Memory used: {(mem2_beforeGC - mem2_initial) / 1024:N0} KB");
			Console.WriteLine($"🗑️  After GC: {(mem2_afterGC - mem2_initial) / 1024:N0} KB");

			// مقایسه
			Console.WriteLine("\n📊 COMPARISON:");
			Console.WriteLine($"🚀 Speed improvement: {((double)sw1.ElapsedMilliseconds / sw2.ElapsedMilliseconds):F2}x");
			var memImprovement = (double)(mem1_beforeGC - mem1_initial) / (mem2_beforeGC - mem2_initial);
			Console.WriteLine($"💾 Memory improvement: {memImprovement:F2}x less allocation");
		}

		// تست طولانی برای بررسی Memory Leak واقعی - 20 round با 2000 پیام هر round
		public void LongRunningLeakTest()
		{
			Console.WriteLine("\n🔄 Starting Long Running Leak Test (20 rounds × 2000 messages)...");

			for (int round = 1; round <= 20; round++)
			{
				Console.WriteLine($"\n--- Round {round} ---");
				var roundStart = GC.GetTotalMemory(false);

				// هر round، 2000 پیام
				for (int i = 0; i < 2000; i++)
				{
					_reusableDict.Clear();
					_reusableDict["Round"] = round;
					_reusableDict["Index"] = i;

					// ترکیبی از انواع مختلف log
					if (i % 3 == 0)
						_logger.Information("Round {Round} message {Index}", _reusableDict);
					else if (i % 3 == 1)
						_logger.Warning("Round {Round} warning {Index}", _reusableDict);
					else
						_logger.Error("Round {Round} error {Index}", _reusableDict, _testException);
				}

				var beforeGC = GC.GetTotalMemory(false);
				GC.Collect();
				GC.WaitForPendingFinalizers();
				GC.Collect();
				var afterGC = GC.GetTotalMemory(false);

				Console.WriteLine($"Memory growth: {(beforeGC - roundStart) / 1024:N0} KB");
				Console.WriteLine($"After GC: {(afterGC - roundStart) / 1024:N0} KB");

				// اگر بعد از GC هنوز memory زیاد باشه، leak داریم
				if ((afterGC - roundStart) > 5 * 1024 * 1024) // بیش از 5MB
				{
					Console.WriteLine($"⚠️  Potential memory leak detected in round {round}!");
				}

				// کمی صبر کنیم
				System.Threading.Thread.Sleep(200);
			}
		}

		public void Cleanup()
		{
			_reusableDict.Clear();
			Console.WriteLine("\n🧹 Cleanup completed");
		}
	}
}