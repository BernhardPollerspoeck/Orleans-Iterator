using Orleans.Runtime;

namespace Orleans.Iterator.Abstraction;

public interface IIteratorGrain<TGrainInterface> : IGrainWithStringKey
    where TGrainInterface : IGrain
{
    Task<GrainId?> GetNextItem();
    Task<bool> Initialize(params string[] storeName);
    Task DisposeAsync();
}