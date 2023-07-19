namespace Orleans.Iterator.Abstraction;

/// <summary>
/// The main injected Iterator service that creates, configures and returns the readers
/// </summary>
public interface IGrainIterator
{
    /// <summary>
    /// Returns a preconfigured reader for the current persistence provider.
    /// </summary>
    /// <typeparam name="TGrainInterface"></typeparam>
    /// <param name="grainTypeString"></param>
    /// <returns></returns>
    Task<IIterativeGrainReader> GetReader<TGrainInterface>(string grainTypeString)
        where TGrainInterface : IGrain;
    //TODO: Document
}
