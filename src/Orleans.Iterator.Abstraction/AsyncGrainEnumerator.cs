using Orleans.Runtime;

namespace Orleans.Iterator.Abstraction;

public class AsyncGrainEnumerator<TGrainInterface>(IIteratorGrain<TGrainInterface> grain, params GrainDescriptor[] grainDescriptions) : IAsyncEnumerator<GrainId>
    where TGrainInterface : IGrain
{
    #region fields
    private bool _initialized;
    private GrainId? _current;
    #endregion

    #region IAsyncEnumerator<GrainId>
    public GrainId Current => _current ?? throw new InvalidOperationException("Not allowed null value in iterator");

    public async ValueTask<bool> MoveNextAsync()
    {
        if (!_initialized)
        {
            await grain.Initialize(grainDescriptions);
            _initialized = true;
        }
        _current = await grain.GetNextItem();
        return _current.HasValue;
    }
    public async ValueTask DisposeAsync()
    {
        await grain.DisposeAsync();
        GC.SuppressFinalize(this);
    }
    #endregion
}