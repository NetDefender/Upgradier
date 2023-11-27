namespace Upgradier.Core;

public sealed class UpdateResultTaskBuffer
{
    public static readonly int MaxValue = Environment.ProcessorCount * 20;

    public const int MinValue = 1;

    private readonly List<Task<UpdateResult>> _parallelTaskBuffer;
    private int _parallelism;
    private readonly LogAdapter _logger;

    public UpdateResultTaskBuffer(int parallelism, LogAdapter _logger)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(parallelism, MinValue);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(parallelism, MaxValue);
        _parallelTaskBuffer = new List<Task<UpdateResult>>(parallelism);
        _parallelism = parallelism;
        _logger = _logger;
    }

    public bool TryAdd(Task<UpdateResult> task)
    {
        if (_parallelTaskBuffer.Count < _parallelism)
        {
            _parallelTaskBuffer.Add(task);
            _logger.LogTaskBufferAddSuccessfully(_parallelTaskBuffer.Count);
            return true;
        }

        _logger.LogTaskBufferFull(_parallelTaskBuffer.Count);

        return false;
    }

    public void Add(Task<UpdateResult> task)
    {
        _parallelTaskBuffer.Add(task);
        _logger.LogTaskBufferAddSuccessfully(_parallelTaskBuffer.Count);
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