using Orleans.Runtime;

namespace Orleans.Iterator.Abstraction;

public class GrainIterator<TGrainInterface> : IGrainIterator
	where TGrainInterface : IGrain
{
	#region fields
	private readonly GrainDescriptor[] _grainDescriptions;
	private readonly IIteratorGrain<TGrainInterface> _grain;
	#endregion

	#region ctor
	public GrainIterator(IClusterClient clusterClient, params GrainDescriptor[] grainDescriptions)
	{
		_grain = clusterClient.GetGrain<IIteratorGrain<TGrainInterface>>(Guid.NewGuid().ToString());
		_grainDescriptions = grainDescriptions;
	}
	#endregion

	#region IAsyncEnumerable<GrainId>
	public IAsyncEnumerator<GrainId> GetAsyncEnumerator(CancellationToken cancellationToken = default)
	{
		return new AsyncGrainEnumerator<TGrainInterface>(_grain, _grainDescriptions);
	}
	#endregion
}
