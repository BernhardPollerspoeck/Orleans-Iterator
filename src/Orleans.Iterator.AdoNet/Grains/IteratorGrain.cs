using Orleans.Iterator.Abstraction;
using Orleans.Iterator.Abstraction.Server;
using Orleans.Runtime;

namespace Orleans.Iterator.AdoNet.Grains;
public class IteratorGrain<TGrainInterface> : Grain, IIteratorGrain<TGrainInterface>
    where TGrainInterface : IGrain
{
    #region fields
    private readonly IServerGrainIterator _serverGrainIterator;
    private IIterativeServerGrainReader? _reader;
    private IEnumerator<GrainId?>? _enumerator;
    #endregion

    #region ctor
    public IteratorGrain(IServerGrainIterator serverGrainIterator)
    {
        _serverGrainIterator = serverGrainIterator;
    }
    #endregion

    #region IIteratorGrain<TGrainInterface>
    public async Task<bool> Initialize(params string[] storeName)
    {
        try
        {
            _reader = await _serverGrainIterator.GetReader<TGrainInterface>(storeName);
            await _reader.StartRead(CancellationToken.None);
            _enumerator = _reader.GetEnumerator();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public Task<GrainId?> GetNextItem()
    {
        return _enumerator?.MoveNext() is true && _enumerator.Current.HasValue
            ? Task.FromResult(_enumerator.Current)
            : Task.FromResult<GrainId?>(null);
    }

    public async Task DisposeAsync()
    {
        if (_reader is not null)
        {
            await _reader.StopRead(CancellationToken.None);
        }
        _enumerator?.Dispose();
        _reader = null;
        _enumerator = null;
    }
    #endregion
}
