using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Iterator.Abstraction;

namespace Orleans.Iterator.AdoNet.Extensions;
public static class HostExtensions
{

    public static ISiloBuilder UseAdoNetGrainIterator(this ISiloBuilder builder, Action<AdoNetGrainIteratorOptions> configureOptions)
    {
        builder.UseAdoNetGrainIterator(c => c.Configure(configureOptions));
        return builder;
    }

    private static ISiloBuilder UseAdoNetGrainIterator(this ISiloBuilder builder, Action<OptionsBuilder<AdoNetGrainIteratorOptions>> configureOptions)
    {
        configureOptions?.Invoke(builder.Services.AddOptions<AdoNetGrainIteratorOptions>());
        builder.Services.AddSingleton<IGrainIterator, AdoGrainIterator>();
        return builder;
    }

}
