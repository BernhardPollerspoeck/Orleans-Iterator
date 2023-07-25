using Microsoft.Extensions.DependencyInjection;

namespace Orleans.Iterator.Abstraction;

public class IteratorFactory : IIteratorFactory
{
    #region fields
    private readonly IServiceProvider _serviceProvider;
    #endregion

    #region ctor
    public IteratorFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    #endregion

    #region IIteratorFactory
    public IGrainIterator CreateIterator<TGrainInterface>(string storeName)
        where TGrainInterface : IGrain
    {
        var client = _serviceProvider.GetRequiredService<IClusterClient>();
        return new GrainIterator<TGrainInterface>(client, storeName);
    }
    #endregion
}
