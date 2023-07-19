using Orleans.Iterator.AdoNet.MainPackageCode;

namespace Orleans.Iterator.AdoNet;

public class AdoNetGrainIteratorOptions
{
    /// <summary>
    /// The default ADO.NET invariant used for storage if none is given. 
    /// </summary>
    public const string DEFAULT_ADONET_INVARIANT = AdoNetInvariants.InvariantNameSqlServer;


    /// <summary>
    /// Connection string for AdoNet storage.
    /// </summary>
    public required string ConnectionString { get; set; }
    /// <summary>
    /// The invariant name for storage.
    /// </summary>
    public string Invariant { get; set; } = DEFAULT_ADONET_INVARIANT;
}

