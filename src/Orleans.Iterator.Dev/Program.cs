using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orleans.Configuration;
using Orleans.Iterator.Dev.Grains;
using Orleans.Iterator.Abstraction;
using Orleans.Iterator.Abstraction.Abstraction;

var builder = Host.CreateDefaultBuilder(args);

const string? CONNECTION = "server=localhost;database=orleansIterator;user=root;password=unsecure1Admin";

builder.UseOrleansClient(clientBuilder =>
{
    var invariant = "MySql.Data.MySqlClient";
    var connectionString = CONNECTION;

    clientBuilder.UseAdoNetClustering(options =>
    {
        options.Invariant = invariant;
        options.ConnectionString = connectionString;
    })
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

var grain = client.GetGrain<IReverseGrain>("aaa");
var rev = await grain.Reverse();



//with this iterator factory you can create a iterator on the server to be consumed at the client.
var iteratorFactory = host.Services.GetRequiredService<IIteratorFactory>();
var iterator = iteratorFactory.CreateIterator<IReverseGrain>("Reverserino");

await foreach (var entry in iterator)
{
    Console.WriteLine($"ID: {entry}");
}



Console.ReadLine();