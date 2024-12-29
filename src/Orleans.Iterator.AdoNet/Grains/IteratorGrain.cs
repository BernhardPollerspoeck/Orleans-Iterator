using Orleans.Iterator.Abstraction;
using Orleans.Iterator.Abstraction.Server;
using Orleans.Runtime;

namespace Orleans.Iterator.AdoNet.Grains;
public class IteratorGrain<TGrainInterface>(IServerGrainIterator serverGrainIterator) : Grain, IIteratorGrain<TGrainInterface>
    where TGrainInterface : IGrain
{

    #region fields
    private IIterativeServerGrainReader? _reader;
    private IEnumerator<GrainId?>? _enumerator;
    #endregion

    #region IIteratorGrain<TGrainInterface>
    public async Task Initialize(params GrainDescriptor[] grainDescriptions)
    {
        _reader = await serverGrainIterator.GetReader<TGrainInterface>(grainDescriptions);
        await _reader.StartRead(CancellationToken.None);
        _enumerator = _reader.GetEnumerator();
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
