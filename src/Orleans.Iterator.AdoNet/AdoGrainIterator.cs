using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Iterator.Abstraction.Server;

namespace Orleans.Iterator.AdoNet;
public class AdoGrainIterator : IServerGrainIterator
{
    #region fields
    private readonly IServiceProvider _serviceProvider;
    #endregion

    #region ctor
    public AdoGrainIterator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    #endregion

    #region IServerGrainIterator
    public Task<IIterativeServerGrainReader> GetReader<TGrainInterface>(string grainTypeString) where TGrainInterface : IGrain
    {
        var storageOptions = _serviceProvider.GetRequiredService<IOptions<AdoNetGrainIteratorOptions>>();
        var clusterOptions = _serviceProvider.GetRequiredService<IOptions<ClusterOptions>>();
        return Task.FromResult(
            (IIterativeServerGrainReader)new AdoIterativeGrainReader<TGrainInterface>(
                storageOptions,
                clusterOptions,
                _serviceProvider,
                grainTypeString));
    }
    #endregion
}
