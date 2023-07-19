using Orleans.Runtime;
using System.Collections;

namespace Orleans.Iterator.Abstraction;

/// <summary>
/// A Reader for the given provider that allows to iterate over all grains of a given type
/// The Reader itself should only be used for iterating once and then be disposed.
/// </summary>
/// <typeparam name="GrainId"></typeparam>
public interface IIterativeGrainReader
    : IEnumerable<GrainId>
    , IEnumerable
{

    /// <summary>
    /// Signals if the reader is able to be used.
    /// </summary>
    bool ReadAllowed { get; }
    
    //TODO: document
    Task<bool> StartRead(CancellationToken cancellationToken);

    //TODO: document
    Task StopRead(CancellationToken cancellationToken);
}
