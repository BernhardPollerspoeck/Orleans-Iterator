using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orleans.Configuration;
using Orleans.Iterator.Dev.Grains;
using Orleans.Iterator.Abstraction;
using Orleans.Iterator.Abstraction.Abstraction;
using Orleans.Runtime;

var configuration = new ConfigurationBuilder()
	.SetBasePath(Directory.GetCurrentDirectory())
	.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
	.AddEnvironmentVariables()
	.AddUserSecrets<Program>()
	.Build();

var storageType = EStorageType.AzureBlob;
var builder = Host.CreateDefaultBuilder(args);

#region configuration
builder.UseOrleansClient(clientBuilder =>
{
	switch (storageType)
	{
		case EStorageType.AdoNet:
			ConfigureAdoNet(clientBuilder, configuration);
			break;

		case EStorageType.AzureBlob:
			ConfigureAzureBlob(clientBuilder, configuration);
			break;
	}

	clientBuilder
		.Configure<ClusterOptions>(o =>
		{
			o.ClusterId = "iterator";
			o.ServiceId = "tester";
		})
		.UseIterator()
	;
});
#endregion

var host = builder.Build();
await host.StartAsync();

#region data prep
var client = host.Services.GetRequiredService<IClusterClient>();

await client.GetGrain<IReverseGrain>(GrainId.Create("Reversed2", "abc")).Reverse();
await client.GetGrain<IReverseGrain>(GrainId.Create("Reversed2", "aaa")).Reverse();
await client.GetGrain<IReverseGrain>(GrainId.Create("Reversed2", "ctp")).Reverse();
await client.GetGrain<IReverseGrain>(GrainId.Create("Reverse2", "abc")).Reverse();
await client.GetGrain<IReverseGrain>(GrainId.Create("Reverse2", "aaa")).Reverse();
await client.GetGrain<IReverseGrain>(GrainId.Create("Reverse2", "ctp")).Reverse();
#endregion

//with this iterator factory you can create a iterator on the server to be consumed at the client.
var grainDescriptors = new GrainDescriptor[]
{
	new("Reverse2", "Reverse"),
	new("Reversed2", "Reversed")
};

var iteratorFactory = host.Services.GetRequiredService<IIteratorFactory>();
var iterator = iteratorFactory
	.CreateIterator<IReverseGrain>(grainDescriptors);

await foreach (var entry in iterator)
{
	Console.WriteLine($"ID: {entry}");
}

Console.ReadLine();

#region configuration
void ConfigureAdoNet(IClientBuilder clientBuilder, IConfiguration configuration)
{
	var adoNetConnectionString = configuration["AdoNet:ConnectionString"] ?? "";
	var adoNetInvariant = configuration["AdoNet:Invariant"] ?? "";

	clientBuilder.UseAdoNetClustering(options =>
	{
		options.Invariant = adoNetInvariant;
		options.ConnectionString = adoNetConnectionString;
	});
}

void ConfigureAzureBlob(IClientBuilder clientBuilder, IConfiguration configuration)
{
	var azureStorageConnectionString = configuration["AzureStorage:ConnectionString"] ?? "";
	clientBuilder.UseAzureStorageClustering(options =>
	{
		options.ConfigureTableServiceClient(azureStorageConnectionString);
	});
}
#endregion
