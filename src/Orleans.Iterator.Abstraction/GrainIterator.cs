using Orleans.Runtime;

namespace Orleans.Iterator.Abstraction;

public class GrainIterator<TGrainInterface>(IClusterClient clusterClient, params GrainDescriptor[] grainDescriptions) : IGrainIterator
	where TGrainInterface : IGrain
{

    #region fields
    private readonly IIteratorGrain<TGrainInterface> _grain = clusterClient.GetGrain<IIteratorGrain<TGrainInterface>>(Guid.NewGuid().ToString());
    #endregion

    #region IAsyncEnumerable<GrainId>
    public IAsyncEnumerator<GrainId> GetAsyncEnumerator(CancellationToken cancellationToken = default)
	{
		return new AsyncGrainEnumerator<TGrainInterface>(_grain, grainDescriptions);
	}
	#endregion
}
