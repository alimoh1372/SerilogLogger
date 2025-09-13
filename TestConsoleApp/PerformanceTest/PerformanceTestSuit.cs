using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SerilogLogger.Abstraction.LoggerInterface;
using SerilogLogger.Implementation;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TestConsoleApp.PerformanceTest
{
	public class PerformanceTestSuite
	{
		private ILog _logger = null!;
		private readonly Dictionary<string, object?> _reusableDict = new(8);
		private Exception _testException = null!;
		private string _smallMessage = string.Empty;
		private string _mediumMessage = string.Empty;
		private string _largeMessage = string.Empty;

		// Performance tracking
		private readonly List<PerformanceResult> _results = new();

		public void Setup()
		{
			Console.WriteLine("🔧 Setting up Performance Test Suite...");

			var configuration = new ConfigurationBuilder()
				.AddJsonFile("LogConfiguration.json", optional: false, reloadOnChange: true)
				.Build();

			var services = new ServiceCollection();

			services.AddLoggerDependencies(configuration);

			var provider = services.BuildServiceProvider();

			_logger = provider.GetRequiredService<ILog>();

			// Pre-generate test data
			_testException = new InvalidOperationException("Performance test exception");
			_smallMessage = "Small log message";
			_mediumMessage = "Medium log message with some additional context and data";
			_largeMessage = new string('X', 1000); // 1KB

			// Warm up JIT and logger
			WarmUp();
		}

		private void WarmUp()
		{
			Console.WriteLine("🔥 Warming up JIT and logger...");
			for (int i = 0; i < 1000; i++)
			{
				_reusableDict.Clear();
				_reusableDict["WarmupIndex"] = i;
				_logger.Information("Warmup message {WarmupIndex}", _reusableDict);
			}

			// Force GC to start clean
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
			Console.WriteLine("✅ Warmup completed");
		}

		// ===================================
		// Single-threaded Performance Tests
		// ===================================

		public void RunSingleThreadedTests()
		{
			Console.WriteLine("\n🚀 Running Single-threaded Performance Tests");
			Console.WriteLine("================================================");

			TestSmallMessages();
			TestMediumMessages();
			TestLargeMessages();
			TestWithExceptions();
			TestMixedWorkload();
			TestDictionaryAllocationComparison();
		}

		private void TestSmallMessages()
		{
			const int iterations = 1000;
			Console.WriteLine($"\n📝 Small Messages Test ({iterations:N0} messages)");

			var stopwatch = Stopwatch.StartNew();
			var initialMemory = GC.GetTotalMemory(false);

			for (int i = 0; i < iterations; i++)
			{
				_reusableDict.Clear();
				_reusableDict["MessageId"] = i;
				_logger.Information(_smallMessage + " {MessageId}", _reusableDict);
			}

			stopwatch.Stop();
			var finalMemory = GC.GetTotalMemory(false);

			var result = new PerformanceResult
			{
				TestName = "Small Messages",
				Iterations = iterations,
				ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
				MemoryUsedKB = (finalMemory - initialMemory) / 1024,
				MessagesPerSecond = iterations / (stopwatch.ElapsedMilliseconds / 1000.0)
			};

			_results.Add(result);
			PrintResult(result);
		}

		private void TestMediumMessages()
		{
			const int iterations = 1000;
			Console.WriteLine($"\n📄 Medium Messages Test ({iterations:N0} messages)");

			var stopwatch = Stopwatch.StartNew();
			var initialMemory = GC.GetTotalMemory(false);

			for (int i = 0; i < iterations; i++)
			{
				_reusableDict.Clear();
				_reusableDict["UserId"] = 1000 + i;
				_reusableDict["Action"] = "UserAction";
				_reusableDict["SessionId"] = Guid.NewGuid().ToString("N")[..8]; // Short GUID
				_logger.Information(_mediumMessage + " {UserId} {Action} {SessionId}", _reusableDict);
			}

			stopwatch.Stop();
			var finalMemory = GC.GetTotalMemory(false);

			var result = new PerformanceResult
			{
				TestName = "Medium Messages",
				Iterations = iterations,
				ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
				MemoryUsedKB = (finalMemory - initialMemory) / 1024,
				MessagesPerSecond = iterations / (stopwatch.ElapsedMilliseconds / 1000.0)
			};

			_results.Add(result);
			PrintResult(result);
		}

		private void TestLargeMessages()
		{
			const int iterations = 1000;
			Console.WriteLine($"\n📋 Large Messages Test ({iterations:N0} messages)");

			var stopwatch = Stopwatch.StartNew();
			var initialMemory = GC.GetTotalMemory(false);

			for (int i = 0; i < iterations; i++)
			{
				_reusableDict.Clear();
				_reusableDict["LargeData"] = _largeMessage;
				_reusableDict["Index"] = i;
				_logger.Error("Large message {LargeData} at {Index}", _reusableDict);
			}

			stopwatch.Stop();
			var finalMemory = GC.GetTotalMemory(false);

			var result = new PerformanceResult
			{
				TestName = "Large Messages",
				Iterations = iterations,
				ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
				MemoryUsedKB = (finalMemory - initialMemory) / 1024,
				MessagesPerSecond = iterations / (stopwatch.ElapsedMilliseconds / 1000.0)
			};

			_results.Add(result);
			PrintResult(result);
		}

		private void TestWithExceptions()
		{
			const int iterations = 1000;
			Console.WriteLine($"\n⚠️  Exception Logging Test ({iterations:N0} exceptions)");

			var stopwatch = Stopwatch.StartNew();
			var initialMemory = GC.GetTotalMemory(false);

			for (int i = 0; i < iterations; i++)
			{
				_reusableDict.Clear();
				_reusableDict["ErrorId"] = i;
				_reusableDict["Component"] = "TestComponent";
				_logger.Error("Exception occurred {ErrorId} in {Component}", _reusableDict, _testException);
			}

			stopwatch.Stop();
			var finalMemory = GC.GetTotalMemory(false);

			var result = new PerformanceResult
			{
				TestName = "Exception Logging",
				Iterations = iterations,
				ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
				MemoryUsedKB = (finalMemory - initialMemory) / 1024,
				MessagesPerSecond = iterations / (stopwatch.ElapsedMilliseconds / 1000.0)
			};

			_results.Add(result);
			PrintResult(result);
		}

		private void TestMixedWorkload()
		{
			const int iterations = 1000;
			Console.WriteLine($"\n🔀 Mixed Workload Test ({iterations:N0} mixed messages)");

			var stopwatch = Stopwatch.StartNew();
			var initialMemory = GC.GetTotalMemory(false);

			for (int i = 0; i < iterations; i++)
			{
				_reusableDict.Clear();

				switch (i % 4)
				{
					case 0: // Information
						_reusableDict["Info"] = $"Info-{i}";
						_logger.Information("Information log {Info}", _reusableDict);
						break;
					case 1: // Warning
						_reusableDict["Warning"] = $"Warn-{i}";
						_logger.Warning("Warning log {Warning}", _reusableDict);
						break;
					case 2: // Error
						_reusableDict["Error"] = $"Error-{i}";
						_logger.Error("Error log {Error}", _reusableDict);
						break;
					case 3: // Exception
						_reusableDict["ExceptionId"] = i;
						_logger.Error("Exception log {ExceptionId}", _reusableDict, _testException);
						break;
				}
			}

			stopwatch.Stop();
			var finalMemory = GC.GetTotalMemory(false);

			var result = new PerformanceResult
			{
				TestName = "Mixed Workload",
				Iterations = iterations,
				ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
				MemoryUsedKB = (finalMemory - initialMemory) / 1024,
				MessagesPerSecond = iterations / (stopwatch.ElapsedMilliseconds / 1000.0)
			};

			_results.Add(result);
			PrintResult(result);
		}

		private void TestDictionaryAllocationComparison()
		{
			const int iterations = 1000;
			Console.WriteLine($"\n⚖️  Dictionary Allocation Comparison ({iterations:N0} each)");

			// Test 1: New Dictionary (old way)
			Console.WriteLine("\n--- New Dictionary Every Time ---");
			var sw1 = Stopwatch.StartNew();
			var mem1 = GC.GetTotalMemory(false);

			for (int i = 0; i < iterations; i++)
			{
				var newDict = new Dictionary<string, object?> { ["Index"] = i, ["Type"] = "NewDict" };
				_logger.Information("New dict message {Index} {Type}", newDict);
			}

			sw1.Stop();
			var mem1Final = GC.GetTotalMemory(false);

			// Test 2: Reused Dictionary (new way)
			Console.WriteLine("\n--- Reused Dictionary ---");
			var sw2 = Stopwatch.StartNew();
			var mem2 = GC.GetTotalMemory(false);

			for (int i = 0; i < iterations; i++)
			{
				_reusableDict.Clear();
				_reusableDict["Index"] = i;
				_reusableDict["Type"] = "ReusedDict";
				_logger.Information("Reused dict message {Index} {Type}", _reusableDict);
			}

			sw2.Stop();
			var mem2Final = GC.GetTotalMemory(false);

			// Results
			var newDictResult = new PerformanceResult
			{
				TestName = "New Dictionary",
				Iterations = iterations,
				ElapsedMilliseconds = sw1.ElapsedMilliseconds,
				MemoryUsedKB = (mem1Final - mem1) / 1024,
				MessagesPerSecond = iterations / (sw1.ElapsedMilliseconds / 1000.0)
			};

			var reusedDictResult = new PerformanceResult
			{
				TestName = "Reused Dictionary",
				Iterations = iterations,
				ElapsedMilliseconds = sw2.ElapsedMilliseconds,
				MemoryUsedKB = (mem2Final - mem2) / 1024,
				MessagesPerSecond = iterations / (sw2.ElapsedMilliseconds / 1000.0)
			};

			_results.Add(newDictResult);
			_results.Add(reusedDictResult);

			PrintResult(newDictResult);
			PrintResult(reusedDictResult);

			// Comparison
			var speedImprovement = newDictResult.MessagesPerSecond / reusedDictResult.MessagesPerSecond;
			var memoryImprovement = (double)newDictResult.MemoryUsedKB / reusedDictResult.MemoryUsedKB;

			Console.WriteLine($"\n📊 COMPARISON:");
			Console.WriteLine($"🚀 Speed: Reused is {speedImprovement:F2}x times {"(faster)"}");
			Console.WriteLine($"💾 Memory: Reused uses {memoryImprovement:F2}x less memory");
		}

		// ===================================
		// Multi-threaded Performance Tests  
		// ===================================

		public void RunMultiThreadedTests()
		{
			Console.WriteLine("\n🧵 Running Multi-threaded Performance Tests");
			Console.WriteLine("==============================================");

			TestConcurrentLogging(2);
			TestConcurrentLogging(4);
			TestConcurrentLogging(8);
			TestHighConcurrencyLogging();
		}

		private void TestConcurrentLogging(int threadCount)
		{
			const int iterationsPerThread = 1000;
			var totalIterations = threadCount * iterationsPerThread;

			Console.WriteLine($"\n👥 Concurrent Logging Test - {threadCount} threads ({totalIterations:N0} total messages)");

			var stopwatch = Stopwatch.StartNew();
			var initialMemory = GC.GetTotalMemory(false);

			var tasks = new Task[threadCount];
			var barrier = new Barrier(threadCount);

			for (int t = 0; t < threadCount; t++)
			{
				int threadId = t;
				tasks[t] = Task.Run(() =>
				{
					var localDict = new Dictionary<string, object?>(4);
					barrier.SignalAndWait(); // همه thread ها همزمان شروع کنند

					for (int i = 0; i < iterationsPerThread; i++)
					{
						localDict.Clear();
						localDict["ThreadId"] = threadId;
						localDict["MessageId"] = i;
						localDict["TotalId"] = threadId * iterationsPerThread + i;

						_logger.Information("Concurrent message from thread {ThreadId} - {MessageId} - {TotalId}", localDict);
					}
				});
			}

			Task.WaitAll(tasks);
			stopwatch.Stop();

			var finalMemory = GC.GetTotalMemory(false);

			var result = new PerformanceResult
			{
				TestName = $"Concurrent ({threadCount} threads)",
				Iterations = totalIterations,
				ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
				MemoryUsedKB = (finalMemory - initialMemory) / 1024,
				MessagesPerSecond = totalIterations / (stopwatch.ElapsedMilliseconds / 1000.0)
			};

			_results.Add(result);
			PrintResult(result);
		}

		private void TestHighConcurrencyLogging()
		{
			int threadCount = Environment.ProcessorCount;
			const int iterationsPerThread = 1000;
			var totalIterations = threadCount * iterationsPerThread;

			Console.WriteLine($"\n🔥 High Concurrency Test - {threadCount} threads (CPU cores) ({totalIterations:N0} total)");

			var stopwatch = Stopwatch.StartNew();
			var initialMemory = GC.GetTotalMemory(false);

			var countdownEvent = new CountdownEvent(threadCount);
			var results = new ConcurrentBag<double>();

			for (int t = 0; t < threadCount; t++)
			{
				int threadId = t;
				ThreadPool.QueueUserWorkItem(_ =>
				{
					var localDict = new Dictionary<string, object?>(6);
					var threadStopwatch = Stopwatch.StartNew();

					for (int i = 0; i < iterationsPerThread; i++)
					{
						localDict.Clear();
						localDict["ThreadId"] = threadId;
						localDict["MessageId"] = i;
						localDict["Timestamp"] = DateTime.Now.Ticks;
						localDict["ProcessId"] = Environment.ProcessId;

						// Mix of different log levels
						switch (i % 3)
						{
							case 0:
								_logger.Information("High concurrency info {ThreadId} {MessageId} {Timestamp} {ProcessId}", localDict);
								break;
							case 1:
								_logger.Warning("High concurrency warning {ThreadId} {MessageId} {Timestamp} {ProcessId}", localDict);
								break;
							case 2:
								_logger.Error("High concurrency error {ThreadId} {MessageId} {Timestamp} {ProcessId}", localDict, _testException);
								break;
						}
					}

					threadStopwatch.Stop();
					results.Add(iterationsPerThread / (threadStopwatch.ElapsedMilliseconds / 1000.0));
					countdownEvent.Signal();
				});
			}

			countdownEvent.Wait();
			stopwatch.Stop();

			var finalMemory = GC.GetTotalMemory(false);
			var avgThreadThroughput = results.Average();

			var result = new PerformanceResult
			{
				TestName = $"High Concurrency ({threadCount} cores)",
				Iterations = totalIterations,
				ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
				MemoryUsedKB = (finalMemory - initialMemory) / 1024,
				MessagesPerSecond = totalIterations / (stopwatch.ElapsedMilliseconds / 1000.0)
			};

			_results.Add(result);
			PrintResult(result);
			Console.WriteLine($"📈 Average per-thread throughput: {avgThreadThroughput:N0} msg/sec");
		}

		// ===================================
		// Stress Tests
		// ===================================

		public void RunStressTests()
		{
			Console.WriteLine("\n💪 Running Stress Tests");
			Console.WriteLine("========================");

			StressTestSustainedThroughput();
			StressTestBurstLogging();
		}

		private void StressTestSustainedThroughput()
		{
			Console.WriteLine("\n⏱️  Sustained Throughput Test (10 seconds)");

			var stopwatch = Stopwatch.StartNew();
			var messageCount = 0;
			var initialMemory = GC.GetTotalMemory(false);

			while (stopwatch.ElapsedMilliseconds < 10000) // 10 seconds
			{
				_reusableDict.Clear();
				_reusableDict["MessageCount"] = messageCount;
				_reusableDict["ElapsedMs"] = stopwatch.ElapsedMilliseconds;

				_logger.Information("Sustained throughput message {MessageCount} at {ElapsedMs}ms", _reusableDict);
				messageCount++;

				if (messageCount % 10000 == 0)
				{
					var currentThroughput = messageCount / (stopwatch.ElapsedMilliseconds / 1000.0);
					Console.WriteLine($"  {stopwatch.ElapsedMilliseconds / 1000:F1}s: {messageCount:N0} messages ({currentThroughput:N0} msg/sec)");
				}
			}

			stopwatch.Stop();
			var finalMemory = GC.GetTotalMemory(false);

			var result = new PerformanceResult
			{
				TestName = "Sustained Throughput (10s)",
				Iterations = messageCount,
				ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
				MemoryUsedKB = (finalMemory - initialMemory) / 1024,
				MessagesPerSecond = messageCount / (stopwatch.ElapsedMilliseconds / 1000.0)
			};

			_results.Add(result);
			PrintResult(result);
		}

		private void StressTestBurstLogging()
		{
			Console.WriteLine("\n💥 Burst Logging Test (10 bursts of 1,000 messages)");

			var totalMessages = 0;
			var totalTime = 0L;
			var initialMemory = GC.GetTotalMemory(false);

			for (int burst = 1; burst <= 10; burst++)
			{
				const int burstSize = 1000;
				Console.WriteLine($"\n  Burst {burst}/10 ({burstSize:N0} messages)...");

				var burstStopwatch = Stopwatch.StartNew();

				for (int i = 0; i < burstSize; i++)
				{
					_reusableDict.Clear();
					_reusableDict["BurstId"] = burst;
					_reusableDict["MessageId"] = i;
					_reusableDict["TotalId"] = totalMessages + i;

					_logger.Information("Burst {BurstId} message {MessageId} total {TotalId}", _reusableDict);
				}

				burstStopwatch.Stop();
				totalMessages += burstSize;
				totalTime += burstStopwatch.ElapsedMilliseconds;

				var burstThroughput = burstSize / (burstStopwatch.ElapsedMilliseconds / 1000.0);
				Console.WriteLine($"    Completed in {burstStopwatch.ElapsedMilliseconds}ms ({burstThroughput:N0} msg/sec)");

				// Short pause between bursts
				Thread.Sleep(500);
			}

			var finalMemory = GC.GetTotalMemory(false);

			var result = new PerformanceResult
			{
				TestName = "Burst Logging (10 bursts)",
				Iterations = totalMessages,
				ElapsedMilliseconds = totalTime,
				MemoryUsedKB = (finalMemory - initialMemory) / 1024,
				MessagesPerSecond = totalMessages / (totalTime / 1000.0)
			};

			_results.Add(result);
			PrintResult(result);
		}

		// ===================================
		// Results and Summary
		// ===================================

		public void PrintSummary()
		{
			Console.WriteLine("\n" + new string('=', 80));
			Console.WriteLine("📊 PERFORMANCE TEST SUMMARY");
			Console.WriteLine(new string('=', 80));

			Console.WriteLine($"{"Test Name",-30} | {"Messages",-10} | {"Time (ms)",-10} | {"Msg/Sec",-12} | {"Memory (KB)",-12}");
			Console.WriteLine(new string('-', 80));

			foreach (var result in _results)
			{
				Console.WriteLine($"{result.TestName,-30} | {result.Iterations,-10:N0} | {result.ElapsedMilliseconds,-10:N0} | {result.MessagesPerSecond,-12:N0} | {result.MemoryUsedKB,-12:N0}");
			}

			Console.WriteLine(new string('=', 80));

			// Best performers
			var fastestTest = _results.OrderByDescending(r => r.MessagesPerSecond).First();
			var mostEfficientTest = _results.OrderBy(r => r.MemoryUsedKB / (double)r.Iterations).First();

			Console.WriteLine($"🏆 Fastest Test: {fastestTest.TestName} ({fastestTest.MessagesPerSecond:N0} msg/sec)");
			Console.WriteLine($"💚 Most Memory Efficient: {mostEfficientTest.TestName} ({(mostEfficientTest.MemoryUsedKB / (double)mostEfficientTest.Iterations * 1024):F2} bytes/msg)");

			var totalMessages = _results.Sum(r => r.Iterations);
			var totalTime = _results.Sum(r => r.ElapsedMilliseconds);
			var overallThroughput = totalMessages / (totalTime / 1000.0);

			Console.WriteLine($"📈 Overall Stats: {totalMessages:N0} messages in {totalTime:N0}ms ({overallThroughput:N0} msg/sec average)");
		}

		private void PrintResult(PerformanceResult result)
		{
			Console.WriteLine($"✅ {result.TestName}:");
			Console.WriteLine($"   ⏱️  Time: {result.ElapsedMilliseconds:N0} ms");
			Console.WriteLine($"   🚀 Throughput: {result.MessagesPerSecond:N0} messages/second");
			Console.WriteLine($"   💾 Memory: {result.MemoryUsedKB:N0} KB");
			Console.WriteLine($"   📊 Efficiency: {(result.MemoryUsedKB / (double)result.Iterations * 1024):F2} bytes/message");
		}

		public void Cleanup()
		{
			_reusableDict.Clear();
			_results.Clear();

			// Final cleanup
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
		}
	}

	// ===================================
	// Performance Result Model
	// ===================================
	public class PerformanceResult
	{
		public string TestName { get; set; } = string.Empty;
		public int Iterations { get; set; }
		public long ElapsedMilliseconds { get; set; }
		public long MemoryUsedKB { get; set; }
		public double MessagesPerSecond { get; set; }
	}
}
