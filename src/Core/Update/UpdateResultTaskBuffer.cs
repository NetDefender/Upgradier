using Upgradier.Core;

namespace Upgradier.Core;

public sealed class UpdateResultTaskBuffer
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
}