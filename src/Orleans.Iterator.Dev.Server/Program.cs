using Orleans.Configuration;
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
            o.IgnoreNullState = true;
        })
);

var host = builder.Build();
await host.RunAsync();


