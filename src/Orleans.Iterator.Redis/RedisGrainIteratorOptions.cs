namespace Orleans.Iterator.Redis;

public class RedisGrainIteratorOptions
{
    /// <summary>
    /// Connection string for Redis storage.
    /// </summary>
    public required string ConnectionString { get; set; }

    /// <summary>
    /// The database number.
    /// </summary>
    public int? DatabaseNumber { get; set; }

    /// <summary>
    /// Should null states be ignored or returned
    /// </summary>
    public bool IgnoreNullState { get; set; }
}

