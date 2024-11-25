using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Iterator.Abstraction;
using Orleans.Iterator.Abstraction.Server;
using StackExchange.Redis;

namespace Orleans.Iterator.Redis;

public class RedisGrainIterator : IServerGrainIterator
{
    #region fields
    private readonly IServiceProvider _serviceProvider;
    private readonly IOptions<RedisGrainIteratorOptions> _storageOptions;
    private ConfigurationOptions _redisOptions;
    private ConnectionMultiplexer? _connection;
    private IOptions<ClusterOptions> _clusterOptions;
    #endregion

    #region ctor
    public RedisGrainIterator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _storageOptions = _serviceProvider.GetRequiredService<IOptions<RedisGrainIteratorOptions>>();
        _redisOptions = ConfigurationOptions.Parse(_storageOptions.Value.ConnectionString);
        _clusterOptions = _serviceProvider.GetRequiredService<IOptions<ClusterOptions>>();
    }
    #endregion

    #region IServerGrainIterator
    public async Task<IIterativeServerGrainReader> GetReader<TGrainInterface>(params GrainDescriptor[] grainDescriptions) where TGrainInterface : IGrain
    {
        var logger = _serviceProvider.GetRequiredService<ILogger<RedisIterativeGrainReader<TGrainInterface>>>();
        var connection = await EnsureOpenedConnection().ConfigureAwait(false);
        return new RedisIterativeGrainReader<TGrainInterface>(
            connection,
            _storageOptions,
            _clusterOptions,
            logger,
            grainDescriptions);
    }
    #endregion

    #region init helper
    private async Task<ConnectionMultiplexer> EnsureOpenedConnection()
    {
        if (_connection is not null && !_connection.IsConnected)
        {
            try
            {
                await _connection.DisposeAsync().ConfigureAwait(false);
            }
            catch(ObjectDisposedException)
            {
                // Ignore
            }
            finally
            {
                _connection = null;
            }
        }

        if (_connection is null)
        {
            _connection = await ConnectionMultiplexer.ConnectAsync(_redisOptions).ConfigureAwait(false);
        }
        return _connection;
    }
    #endregion
}
