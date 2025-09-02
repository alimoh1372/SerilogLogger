namespace SerilogLogger.Utilities.Utilities;

public class DisposeLogProperties : IDisposable
{
    private readonly List<IDisposable> _disposed = new();

    public void Add(IDisposable disposable) => _disposed.Add(disposable);
    
    public void Dispose() => _disposed.ForEach(d => d.Dispose());
}