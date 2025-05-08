namespace Markwardt;

public static class DisposableExtensions
{
    public static void TryDispose(this object? target)
    {
        if (target is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
    
    public static async ValueTask TryDisposeAsync(this object? target)
    {
        if (target is IAsyncDisposable asyncDisposable)
        {
            await asyncDisposable.DisposeAsync();
        }
        else
        {
            target.TryDispose();
        }
    }
}