namespace Orleans.Iterator.Dev.COPY;

internal class DbConstants
{
    /// <summary>
    /// A query template for union all select
    /// </summary>
    public readonly string UnionAllSelectTemplate;

    /// <summary>
    /// Indicates whether the ADO.net provider does only support synchronous operations.
    /// </summary>
    public readonly bool IsSynchronousAdoNetImplementation;

    /// <summary>
    /// Indicates whether the ADO.net provider does streaming operations natively.
    /// </summary>
    public readonly bool SupportsStreamNatively;

    /// <summary>
    /// Indicates whether the ADO.net provider supports cancellation of commands.
    /// </summary>
    public readonly bool SupportsCommandCancellation;

    /// <summary>
    /// The character that indicates a start escape key for columns and tables that are reserved words.
    /// </summary>
    public readonly char StartEscapeIndicator;

    /// <summary>
    /// The character that indicates an end escape key for columns and tables that are reserved words.
    /// </summary>
    public readonly char EndEscapeIndicator;

    public readonly ICommandInterceptor DatabaseCommandInterceptor;


    public DbConstants(char startEscapeIndicator, char endEscapeIndicator, string unionAllSelectTemplate,
                       bool isSynchronousAdoNetImplementation, bool supportsStreamNatively, bool supportsCommandCancellation, ICommandInterceptor commandInterceptor)
    {
        StartEscapeIndicator = startEscapeIndicator;
        EndEscapeIndicator = endEscapeIndicator;
        UnionAllSelectTemplate = unionAllSelectTemplate;
        IsSynchronousAdoNetImplementation = isSynchronousAdoNetImplementation;
        SupportsStreamNatively = supportsStreamNatively;
        SupportsCommandCancellation = supportsCommandCancellation;
        DatabaseCommandInterceptor = commandInterceptor;
    }
}
