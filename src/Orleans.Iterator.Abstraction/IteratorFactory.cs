using Microsoft.Extensions.DependencyInjection;

namespace Orleans.Iterator.Abstraction;

public class IteratorFactory(IServiceProvider serviceProvider) : IIteratorFactory
{
    #region IIteratorFactory
    public IGrainIterator CreateIterator<TGrainInterface>(params GrainDescriptor[] gainDescriptions)
        where TGrainInterface : IGrain
    {
        var client = serviceProvider.GetRequiredService<IClusterClient>();
        return new GrainIterator<TGrainInterface>(client, gainDescriptions);
    }
    #endregion
}
