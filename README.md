# Orleans.Iterator

This project allows to iterate over all grains that have some state(even nulled state) in persistence available.
The iterator returns only the useable GrainId`s to prevent any activation of grains that are maybe unnessecary.

## Packages
| Package | Information | Version |
|---------|-------------|---------|
| Orleans.Iterator.Abstraction | Core abstraction layer containing all interfaces and provider agnostic implementations used by the client. All Implementation Packages depend on this. | ![Nuget](https://img.shields.io/nuget/v/Orleans.Iterator.Abstraction?logo=NuGet&color=00aa00) |
| Orleans.Iterator.AdoNet | Implements the Reader for the Ado.Net Providers. | ![Nuget](https://img.shields.io/nuget/v/Orleans.Iterator.AdoNet?logo=NuGet&color=00aa00) |
| Orleans.Iterator.Azure | Implements the Reader for Azure Blob Provider. | ![Nuget](https://img.shields.io/nuget/v/Orleans.Iterator.Azure?logo=NuGet&color=00aa00) |

## Contributors

[Berhnard Pollerspöck](https://github.com/BernhardPollerspoeck) The core maintainer of this Project.
[yoDon](https://github.com/yoDon): Contributor and maintainer for **Orleans.Iterator.Azure**


## Sample Explaination

### Server Setup
To configure the server side to be able to use this package you need to use the correct extension to add the required configurations.
The Sample shows all current Providers.
```c#
builder.UseOrleans((hostContext, siloBuilder) =>
    siloBuilder
        .UseAdoNetGrainIterator(o =>
        {
            o.Invariant = adoNetInvariant;
            o.ConnectionString = adoNetConnectionString;
        })
        .UseAzureBlobGrainIterator(o =>
        {
            o.ConnectionString = azureStorageConnectionString;
            o.ContainerName = azureStorageContainerName;
        });
);
```

### Client Setup
To configure the client, you just need to call the 'UseIterator' Extension.
```c#
builder.UseOrleansClient(clientBuilder =>
{
    clientBuilder.UseIterator();
});
```

### Useage
To get a working async enumerator you just need to get a factory, wich is used to request a 1 time useage iterator
```c#
var iteratorFactory = host.Services.GetRequiredService<IIteratorFactory>();
var iterator = iteratorFactory.CreateIterator<IReverseGrain>(new[] 
{
    new("Reverse2", "Reverse"),
});

await foreach (var entry in iterator)
{
    Console.WriteLine($"ID: {entry}");
}
```

## Contribution
Contributions in any way are appechiated (More providers, improvements, documentation or anything else). I Kindly ask you to create a Issue to talk about the planned changes or contact me directly on the Orleans Discord.
