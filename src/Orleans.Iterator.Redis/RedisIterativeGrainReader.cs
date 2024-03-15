using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Iterator.Abstraction;
using Orleans.Iterator.Abstraction.Server;
using Orleans.Runtime;
using StackExchange.Redis;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Orleans.Iterator.Redis;

public class RedisIterativeGrainReader<IGrainInterface> : IIterativeServerGrainReader
    where IGrainInterface : IGrain
{
    #region fields
    private readonly RedisGrainIteratorOptions _options;
    private readonly string _serviceId;
    private readonly GrainDescriptor[] _grainDescriptions;

    [SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Reserved for future logging")]
    private readonly ILogger<RedisIterativeGrainReader<IGrainInterface>> _logger;

    private ConnectionMultiplexer? _connection;
    private IServer? _server;
    private IEnumerable<RedisKey>? _cursor;
    private readonly string _statePrefix;
    private readonly Regex _stateRegex;
    #endregion

    #region ctor
    public RedisIterativeGrainReader(
        IOptions<RedisGrainIteratorOptions> options,
        IOptions<ClusterOptions> clusterOptions,
        ILogger<RedisIterativeGrainReader<IGrainInterface>> logger,
        params GrainDescriptor[] grainDescriptions)
    {
        _options = options.Value;
        _serviceId = clusterOptions.Value.ServiceId;
        _logger = logger;
        _grainDescriptions = grainDescriptions;
        _statePrefix = $"{_serviceId}/state/*";
        _stateRegex = new Regex(
            $"^{Regex.Escape(_serviceId)}/state/(?<GrainId>.*)/(?<StateName>[^/]+)$", RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.Singleline);
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
            _cursor = _server.Keys(database: _options.DatabaseNumber ?? -1, pattern: _statePrefix);
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

            if(
                _stateRegex.Match(key.ToString()) is Match match
                && match.Groups["GrainId"].Value is string grainIdStr
                && match.Groups["StateName"].Value is string stateName
                && GrainId.TryParse(grainIdStr, out GrainId grainId)
                && grainId.Type.ToString() is string grainType
                && _grainDescriptions.Any(desc =>
                    desc.GrainType.Equals(grainType, StringComparison.OrdinalIgnoreCase)
                    && desc.StateName.Equals(stateName, StringComparison.OrdinalIgnoreCase))
                )
            {
                yield return grainId;
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