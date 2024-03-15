using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Iterator.Abstraction;
using Orleans.Iterator.Abstraction.Server;

namespace Orleans.Iterator.Redis;

public class RedisGrainIterator(IServiceProvider serviceProvider) : IServerGrainIterator
{
	#region IServerGrainIterator
	public Task<IIterativeServerGrainReader> GetReader<TGrainInterface>(params GrainDescriptor[] grainDescriptions) where TGrainInterface : IGrain
    {
        var storageOptions = serviceProvider.GetRequiredService<IOptions<RedisGrainIteratorOptions>>();
        var clusterOptions = serviceProvider.GetRequiredService<IOptions<ClusterOptions>>();
        var logger = serviceProvider.GetRequiredService<ILogger<RedisIterativeGrainReader<TGrainInterface>>>();
        return Task.FromResult(
            (IIterativeServerGrainReader)new RedisIterativeGrainReader<TGrainInterface>(
                storageOptions,
                clusterOptions,
                logger,
                grainDescriptions));
    }
    #endregion
}