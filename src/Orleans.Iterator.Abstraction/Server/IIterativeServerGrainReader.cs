using Orleans.Runtime;
using System.Collections;

namespace Orleans.Iterator.Abstraction.Server;

/// <summary>
/// A Reader for the given provider that allows to iterate over all grains of a given type
/// The Reader itself should only be used for iterating once and then be disposed.
/// </summary>
/// <typeparam name="GrainId"></typeparam>
public interface IIterativeServerGrainReader
    : IEnumerable<GrainId?>
    , IEnumerable
{

    /// <summary>
    /// Signals if the reader is able to be used.
    /// </summary>
    bool ReadAllowed { get; }

    /// <summary>
    /// Prepares and opens the connection
    /// </summary>
    /// <param name="cancellationToken">A Token to stop this action</param>
    /// <returns>If the reader is ready to iterate</returns>
    Task<bool> StartRead(CancellationToken cancellationToken);

    /// <summary>
    /// Closes and disposes the underlying connection
    /// </summary>
    /// <param name="cancellationToken">A Token to stop this action</param>
    Task StopRead(CancellationToken cancellationToken);
}
