using Microsoft.Extensions.DependencyInjection;

namespace Orleans.Iterator.Abstraction.Abstraction;
public static class HostExtensions
{
    public static IClientBuilder UseIterator(this IClientBuilder builder)
    {
        builder.Services.AddSingleton<IIteratorFactory, IteratorFactory>();

        return builder;
    }
}
