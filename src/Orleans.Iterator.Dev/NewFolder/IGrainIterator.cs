using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Iterator.Dev.COPY;
using Orleans.Metadata;
using Orleans.Runtime;
using Orleans.Storage;
using System.Collections;
using System.Data.Common;
using System.Text;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Orleans.Iterator.Dev.NewFolder;

/// <summary>
/// The main injected Iterator service that creates, configures and returns the readers
/// </summary>
public interface IGrainIterator
{
    /// <summary>
    /// Returns a preconfigured reader for the current persistence provider.
    /// </summary>
    /// <typeparam name="TGrainInterface"></typeparam>
    /// <returns></returns>
    Task<IIterativeGrainReader> GetReader<TGrainInterface>()
        where TGrainInterface : IAddressable;
}

/// <summary>
/// A Reader for the given provider that allows to iterate over all grains of a given type
/// The Reader itself should only be used for iterating once and then be disposed.
/// </summary>
/// <typeparam name="GrainId"></typeparam>
public interface IIterativeGrainReader
    : IEnumerable<GrainId>
    , IEnumerable
    , IAsyncEnumerable<GrainId>
    , IDisposable
    , IAsyncDisposable
{

    /// <summary>
    /// Signals if the reader is able to be used.
    /// </summary>
    bool ReadAllowed { get; }

    /// <summary>
    /// Initializes the required connection to the current provider
    /// </summary>
    /// <returns>The current state of <see cref="ReadAllowed"/></returns>
    Task<bool> StartRead(CancellationToken cancellationToken);

    /// <summary>
    /// Closes and disposes the connection and open reader to the current provides
    /// </summary>
    Task StopRead(CancellationToken cancellationToken);


}

public class AdoIterativeGrainReader<IGrainInterface> : IIterativeGrainReader
    where IGrainInterface : IGrain
{
    #region fields
    private readonly AdoNetGrainStorageOptions _options;
    private readonly string _serviceId;
    private readonly IServiceProvider _serviceProvider;
    private readonly string _storageTypeString;
    private DbDataReader? _reader;
    private GrainType? _concreteGrainType;
    #endregion

    #region ctor
    public AdoIterativeGrainReader(
        IOptions<AdoNetGrainStorageOptions> options,
        IOptions<ClusterOptions> clusterOptions,
        IServiceProvider serviceProvider,
        string storageTypeString
        )
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

        var interfaceResolver = _serviceProvider.GetRequiredService<GrainInterfaceTypeResolver>();
        var grainTypeResolver = _serviceProvider.GetRequiredService<GrainInterfaceTypeToGrainTypeResolver>();

        var interfaceId = interfaceResolver.GetGrainInterfaceType(typeof(IGrainInterface));
        _concreteGrainType = grainTypeResolver.GetGrainType(interfaceId);


        var connection = DbConnectionFactory.CreateConnection(_options.Invariant, _options.ConnectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
        var command = connection.CreateCommand();

        var hasher = new OrleansDefaultHasher();

        command.AddParameter("@serviceId", _serviceId);
        command.AddParameter("@grainTypeHash", hasher.Hash(Encoding.UTF8.GetBytes(_storageTypeString)));
        command.AddParameter("@grainTypeString", _storageTypeString);

        var query = @"SELECT 
                GrainIdHash, 
                GrainIdN0,  
                GrainIdN1, 
                GrainIdExtensionString 
            FROM orleansstorage            
            WHERE 
            	ServiceId = @serviceId AND @ServiceId IS NOT NULL
            	AND GrainTypeString = @grainTypeString AND grainTypeString IS NOT NULL
                AND GrainTypeHash = @grainTypeHash AND @grainTypeHash IS NOT NULL;";
        command.CommandText = query;

        _reader = command.ExecuteReader();

        return ReadAllowed;
    }
    public Task StopRead(CancellationToken cancellationToken)
    {
        //TODO:
        return Task.CompletedTask;
    }
    #endregion

    #region IEnumerable<GrainId>
    public IEnumerator<GrainId> GetEnumerator()
    {
        if (!ReadAllowed)
        {
            yield break;
        }
        while (_reader!.Read())
        {
            //get ado reference
            var idHash = _reader.GetInt32(0);
            var n0 = _reader.GetInt64(1);
            var n1 = _reader.GetInt64(2);
            var extString = _reader.IsDBNull(3) ? null : _reader.GetString(3);

            var grainId = GetGrainId(idHash, n0, n1, extString);
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

    #region converter
    private GrainId? GetGrainId(int idHash, long n0, long n1, string? extString)
    {
        if (n0 is not 0L)//guid key
        {
            var guidData = BitConverter.GetBytes(n0).Concat(BitConverter.GetBytes(n1));

        }
        else if (n1 is not 0L)//long key
        {
            return GrainId.Create(_concreteGrainType.Value, n1.ToString("X").ToLower());
        }
        else//string key
        {
            return GrainId.Create(_concreteGrainType.Value, extString);
        }

        return null;
    }
    #endregion


    #region implement

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public IAsyncEnumerator<GrainId> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }





    public ValueTask DisposeAsync()
    {
        throw new NotImplementedException();
    }
    #endregion


}