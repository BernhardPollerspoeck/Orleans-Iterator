using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Runtime;
using System.Collections;
using Orleans.Iterator.Abstraction.Server;
using Orleans.Iterator.Abstraction;
using StackExchange.Redis;
using Microsoft.Extensions.Logging;

namespace Orleans.Iterator.Redis;

public class RedisIterativeGrainReader<IGrainInterface> : IIterativeServerGrainReader
    where IGrainInterface : IGrain
{
    #region fields
    private readonly RedisGrainIteratorOptions _options;
    private readonly GrainDescriptor[] _grainDescriptions;

    private ILogger<RedisIterativeGrainReader<IGrainInterface>> _logger;
    private ConnectionMultiplexer? _connection;
    private IServer? _server;
    private IEnumerable<RedisKey>? _cursor;
    #endregion

    #region ctor
    public RedisIterativeGrainReader(
        IOptions<RedisGrainIteratorOptions> options,
        ILogger<RedisIterativeGrainReader<IGrainInterface>> logger,
        params GrainDescriptor[] grainDescriptions)
    {
        _options = options.Value;
        _logger = logger;
        _grainDescriptions = grainDescriptions;
    }
    #endregion

    #region IIterativeGrainReader
    public bool ReadAllowed => _cursor is not null;

    public async Task<bool> StartRead(CancellationToken cancellationToken)
    {
        _connection = await CreateOpenedConnection();
        // get the endpoints
        var endpoints = _connection.GetEndPoints();
        foreach (var endpoint in endpoints)
        {
            _server = _connection.GetServer(endpoint);
            _cursor = _server.Keys(database: _options.DatabaseNumber ?? -1, pattern: "");
            if (_cursor is not null)
            {
                return ReadAllowed;
            }
        }

        return ReadAllowed;
    }
    public async Task StopRead(CancellationToken cancellationToken)
    {
        if (_connection is not null)
        {
            await _connection.CloseAsync();
            await _connection.DisposeAsync();
        }
    }
    #endregion

    #region IEnumerable<GrainId>
    public IEnumerator<GrainId?> GetEnumerator()
    {
        if (_cursor is null)
        {
            yield break;
        }
        IEnumerator<RedisKey> enumerator = _cursor.GetEnumerator();

        while (enumerator.MoveNext())
        {
            RedisKey key = enumerator.Current;

            if(GrainId.TryParse(key, out GrainId grainId) && grainId.Type.ToString() is string type)
            {
                if (_grainDescriptions.Any(desc => desc.GrainType.Equals(type, StringComparison.OrdinalIgnoreCase)))
                {
                    yield return grainId;
                }
            }
        }
        yield break;
    }
    #endregion

    #region IEnumerable
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    #endregion

    #region init helper
    private async Task<ConnectionMultiplexer> CreateOpenedConnection()
    {
        var redisOptions = ConfigurationOptions.Parse(_options.ConnectionString);
        var connection = await ConnectionMultiplexer.ConnectAsync(redisOptions).ConfigureAwait(false);
        return connection;
    }
    #endregion
}