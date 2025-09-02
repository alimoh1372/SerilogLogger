// ===================================
// PerformanceTracker.cs - Performance tracking utility
// ===================================
using System.Diagnostics;

namespace SerilogLogger.Implementation.Tools;

public class PerformanceTracker : IDisposable
{
	private readonly Stopwatch _stopwatch;
	private readonly string _operationName;
	private readonly Action<string, long> _onComplete;

	public PerformanceTracker(string operationName, Action<string, long> onComplete)
	{
		_operationName = operationName;
		_onComplete = onComplete;
		_stopwatch = Stopwatch.StartNew();
	}

	public void Dispose()
	{
		_stopwatch.Stop();
		_onComplete?.Invoke(_operationName, _stopwatch.ElapsedMilliseconds);
	}
}