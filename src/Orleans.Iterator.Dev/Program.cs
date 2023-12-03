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

var storageType = configuration["StorageType"] ?? "";
var adoNetConnectionString = configuration["AdoNet:ConnectionString"] ?? "";
var adoNetInvariant = configuration["AdoNet:Invariant"] ?? "";

//
// Note: stateName and storageName are the 1st and 2nd parameters in the
// PersistentState grain attribute, for example:
//
// public ReverseGrain(
//     [PersistentState("Reverse","STORE_NAME")]
//     IPersistentState<ReverseState> state)
// {
//    ...
// }
//
var grainDescriptors = new GrainDescriptor[]
{
	new("Reverse2", "Reverse", "STORE_NAME"),
	new("Reversed2", "Reversed", "STORE_NAME")
};

var builder = Host.CreateDefaultBuilder(args);

builder.UseOrleansClient(clientBuilder =>
{
	switch (storageType)
	{
		case "AdoNet":
			clientBuilder.UseAdoNetClustering(options =>
			{
				options.Invariant = adoNetInvariant;
				options.ConnectionString = adoNetConnectionString;
			});
			break;

	}

	clientBuilder
		.Configure<ClusterOptions>(o =>
		{
			o.ClusterId = "iteartor";
			o.ServiceId = "tester";
		})
		.UseIterator()
	;
});




var host = builder.Build();




await host.StartAsync();

var client = host.Services.GetRequiredService<IClusterClient>();

await client.GetGrain<IReverseGrain>(GrainId.Create("Reversed2", "abc")).Reverse();
await client.GetGrain<IReverseGrain>(GrainId.Create("Reversed2", "aaa")).Reverse();
await client.GetGrain<IReverseGrain>(GrainId.Create("Reversed2", "ctp")).Reverse();
await client.GetGrain<IReverseGrain>(GrainId.Create("Reverse2", "abc")).Reverse();
await client.GetGrain<IReverseGrain>(GrainId.Create("Reverse2", "aaa")).Reverse();
await client.GetGrain<IReverseGrain>(GrainId.Create("Reverse2", "ctp")).Reverse();







//with this iterator factory you can create a iterator on the server to be consumed at the client.
var iteratorFactory = host.Services.GetRequiredService<IIteratorFactory>();
var iterator = iteratorFactory
	.CreateIterator<IReverseGrain>(grainDescriptors);

await foreach (var entry in iterator)
{
	Console.WriteLine($"ID: {entry}");
}



Console.ReadLine();