using Orleans.Runtime;

namespace Orleans.Iterator.Abstraction;

public class GrainIterator<TGrainInterface> : IGrainIterator
    where TGrainInterface : IGrain
{
    #region fields
    private readonly string[] _storeName;
    private readonly IIteratorGrain<TGrainInterface> _grain;
    #endregion

    #region ctor
    public GrainIterator(IClusterClient clusterClient, params string[] storeName)
    {
        _grain = clusterClient.GetGrain<IIteratorGrain<TGrainInterface>>(Guid.NewGuid().ToString());
        _storeName = storeName;
    }
    #endregion

    #region IAsyncEnumerable<GrainId>
    public IAsyncEnumerator<GrainId> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        return new AsyncGrainEnumerator<TGrainInterface>(_grain, _storeName);
    }
    #endregion
}
