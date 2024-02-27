using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Iterator.Abstraction.Server;

namespace Orleans.Iterator.Redis.Extensions;
public static class HostExtensions
{
	public static ISiloBuilder UseRedisGrainIterator(this ISiloBuilder builder, Action<RedisGrainIteratorOptions> configureOptions)
	{
		builder.UseRedisGrainIterator(c => c.Configure(configureOptions));
		return builder;
	}

	private static ISiloBuilder UseRedisGrainIterator(this ISiloBuilder builder, Action<OptionsBuilder<RedisGrainIteratorOptions>> configureOptions)
	{
		configureOptions?.Invoke(builder.Services.AddOptions<RedisGrainIteratorOptions>());
		builder.Services.AddSingleton<IServerGrainIterator, RedisGrainIterator>();
		return builder;
	}
}
