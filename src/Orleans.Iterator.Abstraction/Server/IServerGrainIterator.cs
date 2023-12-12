namespace Orleans.Iterator.Abstraction.Server;

/// <summary>
/// The main injected Iterator service that creates, configures and returns the readers
/// </summary>
public interface IServerGrainIterator
{
    /// <summary>
    /// Returns a preconfigured reader for the current persistence provider.
    /// </summary>
    /// <typeparam name="TGrainInterface"></typeparam>
    /// <param name="grainTypeString"></param>
    /// <returns></returns>
    Task<IIterativeServerGrainReader> GetReader<TGrainInterface>(params GrainDescriptor[] grainDescriptions)
        where TGrainInterface : IGrain;

}
