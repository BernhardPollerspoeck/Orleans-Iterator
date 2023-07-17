using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Iterator.Dev;
using Orleans.Iterator.Dev.NewFolder;
using Orleans.Metadata;
using Orleans.Runtime;
using System.Diagnostics;
using System.Net;

var builder = Host.CreateDefaultBuilder(args);

const string CONNECTION = "server=localhost;database=orleansIterator;user=root;password=unsecure1Admin";

builder.UseOrleans((hostContext, siloBuilder) =>
{
    siloBuilder
    .UseAdoNetClustering(o =>
    {
        o.Invariant = "MySql.Data.MySqlClient";
        o.ConnectionString = CONNECTION;
    })
    .AddAdoNetGrainStorage("STORE_NAME", options =>
    {
        options.Invariant = "MySql.Data.MySqlClient";
        options.ConnectionString = CONNECTION;
    })
    .Configure<ClusterOptions>(o =>
    {
        o.ClusterId = "iteartor";
        o.ServiceId = "tester";
    })
    //.UseGrainIterator("STORE_NAME", options =>
    //{
    //    options.Invariant = "MySql.Data.MySqlClient";
    //    options.ConnectionString = CONNECTION;
    //})
    ;
});


var host = builder.Build();
await host.StartAsync();

var client = host.Services.GetRequiredService<IClusterClient>();

/*
TODO:
- get some raw access to the persistence connection
- create a reader that reads on every iterator request
- close and dispose propperly

- grain TypeHash
- grain type string

COOL:
- define sorting


*/

await client.GetGrain<IReverseGrain>("access").Reverse();
await client.GetGrain<IReverseGrain>("persistence").Reverse();
await client.GetGrain<IReverseGrain>("propperly").Reverse();
await client.GetGrain<IReverseGrain>("iterator").Reverse();
await client.GetGrain<IReverseGrain>("close").Reverse();

await client.GetGrain<IReverseGrain>("abc").Reverse();
await client.GetGrain<IReverseGrain>("ABC").Reverse();

var o1 = new AdoNetGrainStorageOptions
{
    ConnectionString = CONNECTION,
    Invariant = "MySql.Data.MySqlClient"
};
var o2 = new ClusterOptions
{
    ClusterId = "iteartor",
    ServiceId = "tester",
};
var test = new AdoIterativeGrainReader<IReverseGrain>(
    Options.Create(o1),
    Options.Create(o2),
    host.Services,
    "Reverserino");//TODO: resolve

var start = await test.StartRead(CancellationToken.None);

Debug.WriteLine("Iterating:");
foreach (var id in test)
{
    var resolvedGrain = client.GetGrain<IReverseGrain>(id);
    var resolvedGrainReverse = await resolvedGrain.Reverse();
    Debug.WriteLine($"- {id} => [{resolvedGrainReverse}]");
}

await test.StopRead(CancellationToken.None);

Console.ReadLine();



