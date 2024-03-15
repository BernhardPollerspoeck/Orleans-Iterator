# Orleans.Iterator

This project allows to iterate over all grains that have some state(even nulled state) in persistence available.
The iterator returns only the useable GrainId`s to prevent any activation of grains that are maybe unnessecary.

## Packages
| Package | Information | Version |
|---------|-------------|---------|
| Orleans.Iterator.Abstraction | Core abstraction layer containing all interfaces and provider agnostic implementations used by the client. All Implementation Packages depend on this. | ![Nuget](https://img.shields.io/nuget/v/Orleans.Iterator.Abstraction?logo=NuGet&color=00aa00) |
| Orleans.Iterator.AdoNet | Implements the Reader for the Ado.Net Providers. | ![Nuget](https://img.shields.io/nuget/v/Orleans.Iterator.AdoNet?logo=NuGet&color=00aa00) |
| Orleans.Iterator.Azure | Implements the Reader for Azure Blob Provider. | ![Nuget](https://img.shields.io/nuget/v/Orleans.Iterator.Azure?logo=NuGet&color=00aa00) |
| Orleans.Iterator.Redis | Implements the Reader for Redis Provider. | ![Nuget](https://img.shields.io/nuget/v/Orleans.Iterator.Redis?logo=NuGet&color=00aa00) |

## Contributors

 - [Bernhard Pollersp�ck](https://github.com/BernhardPollerspoeck) The core maintainer of this Project.
 - [Don Alvarez](https://github.com/yoDon): Contributor and maintainer for **Orleans.Iterator.Azure**
 - [Hendrik De Vloed](https://github.com/hendrikdevloed): Contributor for **Orleans.Iterator.Redis**

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
        })
        .UseRedisGrainIterator(options =>
        {
            options.ConnectionString = redisConnectionString;
            if (redisDatabaseNumber != int.MinValue)
            {
                options.DatabaseNumber = redisDatabaseNumber;
            }
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

### Usage
To get a working async enumerator you just need to get a factory, which is used to request an iterator.

```c#
var iteratorFactory = host.Services.GetRequiredService<IIteratorFactory>();
var iterator = iteratorFactory.CreateIterator<IReverseGrain>(new[] 
{
    // Example for use with grain showing [GrainType("Reverse2")]
    // and grain constructor showing [PersistentState("Reverse","STORE_NAME")]
    new("Reverse2", "Reverse"),
});

await foreach (var entry in iterator)
{
    Console.WriteLine($"ID: {entry}");
}
```

### Sample Projects: 

1. If you are only using one or the other of AdoNet or Azure Storage, comment out the unused storage types in `Orleans.Iterator.Dev/program.cs` and `Orleans.Iterator.Dev.Server/program.cs`.

2. Open the .NET Secrets file for the `Orleans.Iterator.Dev` project and copy into it the contents of the `Orleans.Iterator.Dev/secrets.template.json` file.

3. Edit the StorageType and ConnectionString properties in the file to match your storage configuration.

4. Open the .NET Secrets file for the `Orleans.Iterator.Dev.Server` project and copy into it the contents of the `Orleans.Iterator.Dev.Server/secrets.template.json` file, editing in your own connection strings as appropriate.

5. Edit the StorageType and ConnectionString properties in the file to match your storage configuration.

6. Start the `Orleans.Iterator.Dev.Server` project, which host the example silo.

7. Start the `Orleans.Iterator.Dev` project, which hosts the example client and uses the iterator to find grains.

## Contribution
Contributions in any way are appechiated (More providers, improvements, documentation or anything else). I Kindly ask you to create a Issue to talk about the planned changes or contact me directly on the Orleans Discord.
