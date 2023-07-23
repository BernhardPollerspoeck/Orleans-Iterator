namespace Orleans.Iterator.AdoNet.QueryProviders;

internal class MySqlQueryProvider : IQueryProvider
{
    public string GetSelectGrainIdQuery()
    {
        return @"SELECT
                GrainIdN0,  
                GrainIdN1, 
                GrainIdExtensionString 
            FROM orleansstorage            
            WHERE 
            	ServiceId = @serviceId AND @ServiceId IS NOT NULL
            	AND GrainTypeString = @grainTypeString AND grainTypeString IS NOT NULL
                AND GrainTypeHash = @grainTypeHash AND @grainTypeHash IS NOT NULL;";
    }
}