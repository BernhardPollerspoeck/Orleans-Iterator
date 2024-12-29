using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Iterator.Abstraction;
using Orleans.Iterator.Abstraction.Server;

namespace Orleans.Iterator.AdoNet;
public class AdoGrainIterator(IServiceProvider serviceProvider) : IServerGrainIterator
{
    #region IServerGrainIterator
    public Task<IIterativeServerGrainReader> GetReader<TGrainInterface>(params GrainDescriptor[] grainDescriptions) where TGrainInterface : IGrain
    {
        var storageOptions = serviceProvider.GetRequiredService<IOptions<AdoNetGrainIteratorOptions>>();
        var clusterOptions = serviceProvider.GetRequiredService<IOptions<ClusterOptions>>();
        return Task.FromResult(
            (IIterativeServerGrainReader)new AdoIterativeGrainReader<TGrainInterface>(
                storageOptions,
                clusterOptions,
				grainDescriptions));
    }
    #endregion
}
