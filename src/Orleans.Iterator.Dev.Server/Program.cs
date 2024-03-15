using Orleans.Configuration;
using Orleans.Iterator.AdoNet.Extensions;
using Orleans.Iterator.Azure.Extensions;
using Orleans.Iterator.Redis.Extensions;
using Orleans.Iterator.Dev.Grains;
using StackExchange.Redis;

var configuration = new ConfigurationBuilder()
	.SetBasePath(Directory.GetCurrentDirectory())
	.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
	.AddEnvironmentVariables()
	.AddUserSecrets<Program>()
	.Build();

var storageType = EStorageType.Redis;
var builder = Host.CreateDefaultBuilder(args);

#region configuration
builder.UseOrleans((hostContext, siloBuilder) =>
{
	switch (storageType)
	{
		case EStorageType.AdoNet:
			ConfigureAdoNet(siloBuilder, configuration);
			break;

		case EStorageType.AzureBlob:
			ConfigureAzureBlob(siloBuilder, configuration);
			break;

		case EStorageType.Redis:
			ConfigureRedis(siloBuilder, configuration);
			break;
	}

	siloBuilder
		.Configure<ClusterOptions>(o =>
		{
			o.ClusterId = "iterator";
			o.ServiceId = "tester";
		});
});
#endregion

var host = builder.Build();
await host.RunAsync();

#region configuration
void ConfigureAdoNet(ISiloBuilder siloBuilder, IConfiguration configuration)
{

	var adoNetConnectionString = configuration["AdoNet:ConnectionString"] ?? "";
	var adoNetInvariant = configuration["AdoNet:Invariant"] ?? "";

	siloBuilder
		.UseAdoNetClustering(o =>
		{
			o.Invariant = adoNetInvariant;
			o.ConnectionString = adoNetConnectionString;
		})
		.AddAdoNetGrainStorage("STORE_NAME", o =>
		{
			o.Invariant = adoNetInvariant;
			o.ConnectionString = adoNetConnectionString;
		})
		.UseAdoNetGrainIterator(o =>
		{
			o.Invariant = adoNetInvariant;
			o.ConnectionString = adoNetConnectionString;
			o.IgnoreNullState = true;
		});
}

static void ConfigureAzureBlob(ISiloBuilder siloBuilder, IConfiguration configuration)
{
	var azureStorageConnectionString = configuration["AzureStorage:ConnectionString"] ?? "";
	var azureStorageContainerName = configuration["AzureStorage:ContainerName"] ?? "";

	siloBuilder
		.UseAzureStorageClustering(o =>
		{
			o.ConfigureTableServiceClient(azureStorageConnectionString);
		})
		.AddAzureBlobGrainStorage("STORE_NAME", o =>
		{
			o.ConfigureBlobServiceClient(azureStorageConnectionString);
		})
		.UseAzureBlobGrainIterator(o =>
		{
			o.ConnectionString = azureStorageConnectionString;
			o.ContainerName = azureStorageContainerName;
		});
}

static void ConfigureRedis(ISiloBuilder siloBuilder, IConfiguration configuration)
{
    var redisConnectionString = configuration["Redis:ConnectionString"] ?? "";
    var redisDatabaseNumber = configuration.GetValue<int>("Redis:DatabaseNumber", int.MinValue);
    var redisConfigurationOptions = ConfigurationOptions.Parse(redisConnectionString);
    if (redisDatabaseNumber != int.MinValue)
    {
        redisConfigurationOptions.DefaultDatabase = redisDatabaseNumber;
    }

    siloBuilder
        .UseRedisClustering(options =>
        {
			options.ConfigurationOptions = redisConfigurationOptions;
        })
        .AddRedisGrainStorage("STORE_NAME", options =>
        {
			options.ConfigurationOptions = redisConfigurationOptions;
        })
        .UseRedisGrainIterator(options =>
        {
			options.ConnectionString = redisConnectionString;
            if (redisDatabaseNumber != int.MinValue)
            {
                options.DatabaseNumber = redisDatabaseNumber;
            }
        });
}
#endregion
