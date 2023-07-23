# Orleans.Iterator

This project allows to iterate over all grains that have some state(even nulled state) in persistence available.

The iterator returns only the useable GrainId`s to prevent any activation of grains that are maybe unnessecary.

Currently this package only supports Ado.Net MySql. Any Contributions are highly apprechiated.
