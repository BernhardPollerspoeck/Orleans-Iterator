using Orleans.Runtime;

namespace Orleans.Iterator.Abstraction;

public class AsyncGrainEnumerator<TGrainInterface> : IAsyncEnumerator<GrainId>
    where TGrainInterface : IGrain
{
    #region fields
    private readonly IIteratorGrain<TGrainInterface> _grain;
    private readonly string _storeName;

    private bool _initialized;
    private GrainId? _current;
    #endregion

    #region ctor
    public AsyncGrainEnumerator(IIteratorGrain<TGrainInterface> grain, string storeName)
    {
        _grain = grain;
        _storeName = storeName;
    }
    #endregion

    #region IAsyncEnumerator<GrainId>
    public GrainId Current => _current ?? throw new InvalidOperationException("Not allowed null value in iterator");

    public async ValueTask<bool> MoveNextAsync()
    {
        if (!_initialized)
        {
            _initialized = await _grain.Initialize(_storeName);
        }
        if (!_initialized)
        {
            throw new InvalidOperationException("Could not initialize async remote iterator");
        }
        _current = await _grain.GetNextItem();
        return _current.HasValue;
    }
    public async ValueTask DisposeAsync()
    {
        await _grain.DisposeAsync();
        GC.SuppressFinalize(this);
    }
    #endregion
}