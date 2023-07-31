namespace Orleans.Iterator.AdoNet.QueryProviders;

internal class MySqlQueryProvider : IQueryProvider
{
    public string GetSelectGrainIdQuery(bool ignoreNullState, int grainTypeStringCount = 1)
    {
        var gts = string.Join(
            ", ",
            Enumerable.Range(1, grainTypeStringCount).Select(x => $"@gts{x}"));
        return @$"SELECT
                GrainIdN0,  
                GrainIdN1, 
                GrainIdExtensionString,
                GrainTypeString
            FROM orleansstorage            
            WHERE 
            	ServiceId = @serviceId AND @ServiceId IS NOT NULL
            	AND GrainTypeString in ({gts}) AND grainTypeString IS NOT NULL
                {(ignoreNullState ? "AND PayloadBinary IS NOT NULL" : string.Empty)};";
    }
}