using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Iterator.Abstraction.Server;
using Orleans.Iterator.Azure.Blob;

namespace Orleans.Iterator.Azure.Extensions;
public static class HostExtensions
{
	public static ISiloBuilder UseAzureBlobGrainIterator(this ISiloBuilder builder, Action<AzureBlobGrainIteratorOptions> configureOptions)
	{
		builder.UseAzureBlobGrainIterator(c => c.Configure(configureOptions));
		return builder;
	}

	private static ISiloBuilder UseAzureBlobGrainIterator(this ISiloBuilder builder, Action<OptionsBuilder<AzureBlobGrainIteratorOptions>> configureOptions)
	{
		configureOptions?.Invoke(builder.Services.AddOptions<AzureBlobGrainIteratorOptions>());
		builder.Services.AddSingleton<IServerGrainIterator, AzureBlobGrainIterator>();
		return builder;
	}
}
