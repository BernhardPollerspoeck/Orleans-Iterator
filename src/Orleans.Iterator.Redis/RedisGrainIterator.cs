﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Iterator.Abstraction;
using Orleans.Iterator.Abstraction.Server;

namespace Orleans.Iterator.Redis;

public class RedisGrainIterator : IServerGrainIterator
{
    #region fields
    private readonly IServiceProvider _serviceProvider;
    #endregion

    #region ctor
    public RedisGrainIterator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    #endregion

    #region IServerGrainIterator
    public Task<IIterativeServerGrainReader> GetReader<TGrainInterface>(params GrainDescriptor[] grainDescriptions) where TGrainInterface : IGrain
    {
        var storageOptions = _serviceProvider.GetRequiredService<IOptions<RedisGrainIteratorOptions>>();
        var clusterOptions = _serviceProvider.GetRequiredService<IOptions<ClusterOptions>>();
        var logger = _serviceProvider.GetRequiredService<ILogger<RedisIterativeGrainReader<TGrainInterface>>>();
        return Task.FromResult(
            (IIterativeServerGrainReader)new RedisIterativeGrainReader<TGrainInterface>(
                storageOptions,
                clusterOptions,
                logger,
                grainDescriptions));
    }
    #endregion
}