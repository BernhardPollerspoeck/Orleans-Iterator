using Orleans.Iterator.AdoNet.MainPackageCode;

namespace Orleans.Iterator.AdoNet.QueryProviders;

internal static class QueryProviderFactory
{
    public static IQueryProvider CreateProvider(string invariant)
    {
        return invariant switch
        {
            AdoNetInvariants.InvariantNameMySql => new MySqlQueryProvider(),

            _ => throw new NotImplementedException($"Provider [{invariant}] is not yet implemented. Please file an issue at: https://github.com/BernhardPollerspoeck/Orleans-Iterator/issues"),
        };
    }
}
