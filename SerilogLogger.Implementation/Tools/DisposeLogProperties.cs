using System;
using System.Collections.Generic;

namespace SerilogLogger.Implementation.Tools
{
	public sealed class DisposeLogProperties : IDisposable
	{
		private List<IDisposable>? _disposables;
		private bool _disposed = false; // Track disposal state

		public void Add(IDisposable disposable)
		{
			if (_disposed)
				throw new ObjectDisposedException(nameof(DisposeLogProperties));

			if (disposable == null) return; // Guard against null

			_disposables ??= new List<IDisposable>(4);
			_disposables.Add(disposable);
		}

		public void Dispose()
		{
			if (_disposed) return;

			if (_disposables != null)
			{
				// Dispose in reverse order for safety
				for (int i = _disposables.Count - 1; i >= 0; i--)
				{
					try
					{
						_disposables[i]?.Dispose();
					}
					catch
					{
						// Ignore disposal errors to prevent masking original exceptions
					}
				}
				_disposables.Clear();
				_disposables = null; // ✅ اهمیت: null کردن reference
			}

			_disposed = true;
			GC.SuppressFinalize(this); // ✅ مهم: جلوگیری از finalization
		}
	}
}