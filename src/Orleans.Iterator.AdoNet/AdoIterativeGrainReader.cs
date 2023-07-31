using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Runtime;
using System.Collections;
using System.Data.Common;
using Orleans.Iterator.AdoNet.MainPackageCode;
using Orleans.Iterator.AdoNet.QueryProviders;
using Orleans.Iterator.Abstraction.Server;

namespace Orleans.Iterator.AdoNet;

public class AdoIterativeGrainReader<IGrainInterface> : IIterativeServerGrainReader
    where IGrainInterface : IGrain
{
    #region fields
    private readonly AdoNetGrainIteratorOptions _options;
    private readonly string _serviceId;
    private readonly string[] _storageTypeString;

    private DbDataReader? _reader;
    private DbConnection? _connection;
    private DbCommand? _command;
    #endregion

    #region ctor
    public AdoIterativeGrainReader(
        IOptions<AdoNetGrainIteratorOptions> options,
        IOptions<ClusterOptions> clusterOptions,
        params string[] storageTypeString)
    {
        _options = options.Value;
        _serviceId = clusterOptions.Value.ServiceId;
        _storageTypeString = storageTypeString;
    }
    #endregion

    #region IIterativeGrainReader
    public bool ReadAllowed => this is { _reader: { HasRows: true, IsClosed: false } };

    public async Task<bool> StartRead(CancellationToken cancellationToken)
    {
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
            var type = _reader.GetString(3);

            var grainId = GetGrainId(n0, n1, extString, type);
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
    private async Task<DbConnection> CreateOpenedConnection()
    {
        var connection = DbConnectionFactory.CreateConnection(_options.Invariant, _options.ConnectionString);
        await connection.OpenAsync();
        return connection;
    }
    private DbCommand CreateCommand(DbConnection connection)
    {
        var command = connection.CreateCommand();
        command.AddParameter("@serviceId", _serviceId);
        for (int i = 1; i <= _storageTypeString.Length; i++)
        {
            command.AddParameter($"@gts{i}", _storageTypeString[i - 1]);
        }
        return command;
    }
    private void SetQuery(DbCommand command)
    {
        var queryProvider = QueryProviderFactory.CreateProvider(_options.Invariant);
        command.CommandText = queryProvider.GetSelectGrainIdQuery(_options.IgnoreNullState, _storageTypeString.Length);
    }
    #endregion

    #region converter
    private static GrainId? GetGrainId(long n0, long n1, string? extString, string grainTypeString)
    {
        if (n0 is not 0L)//guid key
        {
            //TODO: implement
            //var guidData = BitConverter.GetBytes(n0).Concat(BitConverter.GetBytes(n1));

        }
        else if (n1 is not 0L)//long key
        {
            return GrainId.Create(grainTypeString, n1.ToString("X").ToLower());
        }
        else if (extString is not null)//string key
        {
            return GrainId.Create(grainTypeString, extString);
        }

        return null;
    }
    #endregion

}