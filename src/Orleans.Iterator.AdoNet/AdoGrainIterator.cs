using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Iterator.Abstraction;

namespace Orleans.Iterator.AdoNet;
public class AdoGrainIterator : IGrainIterator
{
    private readonly IServiceProvider _serviceProvider;

    public AdoGrainIterator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }


    public Task<IIterativeGrainReader> GetReader<TGrainInterface>(string grainTypeString) where TGrainInterface : IGrain
    {
        var storageOptions = _serviceProvider.GetRequiredService<IOptions<AdoNetGrainIteratorOptions>>();
        var clusterOptions = _serviceProvider.GetRequiredService<IOptions<ClusterOptions>>();
        return Task.FromResult(
            (IIterativeGrainReader)new AdoIterativeGrainReader<TGrainInterface>(
                storageOptions,
                clusterOptions,
                _serviceProvider,
                grainTypeString));

    }
}
