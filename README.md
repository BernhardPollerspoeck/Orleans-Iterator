# Orleans.Iterator

This project allows to iterate over all grains that have some state(even nulled state) in persistence available.
The iterator returns only the useable GrainId`s to prevent any activation of grains that are maybe unnessecary.

## Packages
| Package | Information | Version |
|---------|-------------|---------|
| Orleans.Iterator.Abstraction | Core abstraction layer containing all interfaces. All Implementation Packages depend on this. | ![Nuget](https://img.shields.io/nuget/v/Orleans.Iterator.Abstraction?logo=NuGet&color=00aa00) |
| Orleans.Iterator.AdoNet | Implements the Reader for the Ado.Net Providers. | ![Nuget](https://img.shields.io/nuget/v/Orleans.Iterator.AdoNet?logo=NuGet&color=00aa00) |

## Sample Explaination

### Server Setup
To configure the server side to be able to use this package you need to use the correct extension to add the required configurations.
The Sample shows how a Ado.net Grain Iterator is added.
```c#
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
```

### Client Setup
To configure the client, you just need to call the 'UseIterator' Extension.
```c#
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
    .UseIterator();
});
```

### Useage
To get a working async enumerator you just need to get a factory, wich is used to request a 1 time useage iterator
```c#
var iteratorFactory = host.Services.GetRequiredService<IIteratorFactory>();
var iterator = iteratorFactory.CreateIterator<IReverseGrain>("Reverserino");

await foreach (var entry in iterator)
{
    Console.WriteLine($"ID: {entry}");
}
```

## Contribution
Contributions in any way are appechiated (More providers, improvements, documentation or anything else). I Kindly ask you to create a Issue to talk about the planned changes.
