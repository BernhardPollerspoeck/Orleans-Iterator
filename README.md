# Orleans.Iterator

This project allows to iterate over all grains that have some state(even nulled state) in persistence available.

The iterator returns only the useable GrainId`s to prevent any activation of grains that are maybe unnessecary.

Currently this package only supports Ado.Net MySql. Any Contributions are highly apprechiated.


## This Project offers the following NuGet Packages:
![Nuget](https://img.shields.io/nuget/v/Orleans.Iterator.Abstraction?logo=NuGet&color=00aa00) 'Abstraction' is the core abstraction layer containing all interfaces. All Implementation Packages depend on this.

![Nuget](https://img.shields.io/nuget/v/Orleans.Iterator.AdoNet?logo=NuGet&color=00aa00) 'AdoNed' implements the Reader for the Ado.Net Providers.



