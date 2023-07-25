using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Metadata;
using Orleans.Runtime;
using System.Collections;
using System.Data.Common;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Iterator.AdoNet.MainPackageCode;
using Orleans.Storage;
using Orleans.Iterator.AdoNet.QueryProviders;
using Orleans.Iterator.Abstraction.Server;

namespace Orleans.Iterator.AdoNet;

public class AdoIterativeGrainReader<IGrainInterface> : IIterativeServerGrainReader
    where IGrainInterface : IGrain
{
    #region fields
    private readonly AdoNetGrainIteratorOptions _options;
    private readonly string _serviceId;
    private readonly IServiceProvider _serviceProvider;
    private readonly string _storageTypeString;

    private DbDataReader? _reader;
    private GrainType? _concreteGrainType;
    private DbConnection? _connection;
    private DbCommand? _command;
    #endregion

    #region ctor
    public AdoIterativeGrainReader(
        IOptions<AdoNetGrainIteratorOptions> options,
        IOptions<ClusterOptions> clusterOptions,
        IServiceProvider serviceProvider,
        string storageTypeString)
    {
        _options = options.Value;
        _serviceId = clusterOptions.Value.ServiceId;
        _serviceProvider = serviceProvider;
        _storageTypeString = storageTypeString;
    }
    #endregion

    #region IIterativeGrainReader
    public bool ReadAllowed => this is { _reader: { HasRows: true, IsClosed: false } };

    public async Task<bool> StartRead(CancellationToken cancellationToken)
    {
        ResolveConcreteGrainType();
        _connection = await CreateOpenedConnection();
        _command = CreateCommand(_connection);
        SetQuery(_command);
        _reader = _command.ExecuteReader();

        return ReadAllowed;
    }
    public async Task StopRead(CancellationToken cancellationToken)
    {
        if (_connection is not null)
        {
            await _connection.CloseAsync();
            await _connection.DisposeAsync();
        }
        if (_reader is not null)
        {
            await _reader.DisposeAsync();
        }
        if (_command is not null)
        {
            await _command.DisposeAsync();
        }
    }
    #endregion

    #region IEnumerable<GrainId>
    public IEnumerator<GrainId?> GetEnumerator()
    {
        if (!ReadAllowed)
        {
            yield break;
        }
        while (_reader!.Read())
        {
            var n0 = _reader.GetInt64(0);
            var n1 = _reader.GetInt64(1);
            var extString = _reader.IsDBNull(2) ? null : _reader.GetString(2);

            var grainId = GetGrainId(n0, n1, extString);
            if (grainId.HasValue)
            {
                yield return grainId.Value;
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
    private void ResolveConcreteGrainType()
    {
        var interfaceResolver = _serviceProvider.GetRequiredService<GrainInterfaceTypeResolver>();
        var grainTypeResolver = _serviceProvider.GetRequiredService<GrainInterfaceTypeToGrainTypeResolver>();

        var interfaceId = interfaceResolver.GetGrainInterfaceType(typeof(IGrainInterface));
        _concreteGrainType = grainTypeResolver.GetGrainType(interfaceId);
    }
    private async Task<DbConnection> CreateOpenedConnection()
    {
        var connection = DbConnectionFactory.CreateConnection(_options.Invariant, _options.ConnectionString);
        await connection.OpenAsync();
        return connection;
    }
    private DbCommand CreateCommand(DbConnection connection)
    {
        var hasher = new OrleansDefaultHasher();
        var command = connection.CreateCommand();
        command.AddParameter("@serviceId", _serviceId);
        command.AddParameter("@grainTypeHash", hasher.Hash(Encoding.UTF8.GetBytes(_storageTypeString)));
        command.AddParameter("@grainTypeString", _storageTypeString);
        return command;
    }
    private void SetQuery(DbCommand command)
    {
        var queryProvider = QueryProviderFactory.CreateProvider(_options.Invariant);
        command.CommandText = queryProvider.GetSelectGrainIdQuery();
    }
    #endregion

    #region converter
    private GrainId? GetGrainId(long n0, long n1, string? extString)
    {
        //we did not resolve the concrete type. so we cant return a id based on the type
        if (!_concreteGrainType.HasValue)
        {
            return null;
        }

        if (n0 is not 0L)//guid key
        {
            //TODO: implement
            //var guidData = BitConverter.GetBytes(n0).Concat(BitConverter.GetBytes(n1));

        }
        else if (n1 is not 0L)//long key
        {
            return GrainId.Create(_concreteGrainType.Value, n1.ToString("X").ToLower());
        }
        else if (extString is not null)//string key
        {
            return GrainId.Create(_concreteGrainType.Value, extString);
        }

        return null;
    }
    #endregion

}