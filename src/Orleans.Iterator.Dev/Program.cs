using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orleans.Configuration;
using System.Diagnostics;
using Orleans.Iterator.Dev.Grains;
using Orleans.Iterator.AdoNet.Extensions;

var builder = Host.CreateDefaultBuilder(args);

const string? CONNECTION = "server=localhost;database=orleansIterator;user=root;password=unsecure1Admin";

builder.UseOrleans((hostContext, siloBuilder) =>
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
        .UseAdoNetGrainIterator(o =>
        {
            o.Invariant = "MySql.Data.MySqlClient";
            o.ConnectionString = CONNECTION;
        })
);

var host = builder.Build();
await host.StartAsync();

var client = host.Services.GetRequiredService<IClusterClient>();

await client.GetGrain<IReverseGrain>("access").Reverse();
await client.GetGrain<IReverseGrain>("persistence").Reverse();
await client.GetGrain<IReverseGrain>("propperly").Reverse();
await client.GetGrain<IReverseGrain>("iterator").Reverse();
await client.GetGrain<IReverseGrain>("close").Reverse();
await client.GetGrain<IReverseGrain>("abc").Reverse();


var aggregate = await client
    .GetGrain<IAggregateGrain>("Look in 'DoWork' for iterator useage")
    .DoWork();
Debug.WriteLine(aggregate);


Console.ReadLine();