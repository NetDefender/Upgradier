using Upgradier.Core;

namespace Ugradier.Core;

public sealed class UpdateResultTaskBuffer : IDisposable
{
    public static readonly int MaxValue = Environment.ProcessorCount * 20;

    public const int MinValue = 1;

    private readonly List<Task<UpdateResult>> _parallelTaskBuffer;
    private int _parallelism;
    private bool _isDisposed;

    public UpdateResultTaskBuffer(int parallelism)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(parallelism, MinValue);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(parallelism, MaxValue);
        _parallelTaskBuffer = new List<Task<UpdateResult>>(parallelism);
        _parallelism = parallelism;
    }

    public bool TryAdd(Task<UpdateResult> task)
    {
        if (_parallelTaskBuffer.Count < _parallelism)
        {
            _parallelTaskBuffer.Add(task);
            return true;
        }

        return false;
    }

    public void Add(Task<UpdateResult> task)
    {
        _parallelTaskBuffer.Add(task);
    }

    public async Task<UpdateResult[]> WhenAll()
    {
        return await Task.WhenAll(_parallelTaskBuffer).ConfigureAwait(false);
    }

    public void Clear()
    {
        _parallelTaskBuffer.Clear();
    }

    private void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                Clear();
            }

            _isDisposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}