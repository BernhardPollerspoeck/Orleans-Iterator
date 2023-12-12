using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Iterator.Abstraction;
using Orleans.Iterator.Abstraction.Server;

namespace Orleans.Iterator.Azure.Blob;
public class AzureBlobGrainIterator : IServerGrainIterator
{
    #region fields
    private readonly IServiceProvider _serviceProvider;
    #endregion

    #region ctor
    public AzureBlobGrainIterator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    #endregion

    #region IServerGrainIterator
    public Task<IIterativeServerGrainReader> GetReader<TGrainInterface>(params GrainDescriptor[] grainDescriptions) where TGrainInterface : IGrain
    {
        var storageOptions = _serviceProvider.GetRequiredService<IOptions<AzureBlobGrainIteratorOptions>>();
        return Task.FromResult(
            (IIterativeServerGrainReader)new AzureBlobIterativeGrainReader<TGrainInterface>(
                storageOptions,
				grainDescriptions));
    }
    #endregion
}
