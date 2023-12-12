using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;
using Orleans.Iterator.Abstraction;
using Orleans.Iterator.Abstraction.Server;
using Orleans.Runtime;
using System.Collections;
using System.Text.RegularExpressions;

namespace Orleans.Iterator.Azure.Blob;
public class AzureBlobIterativeGrainReader<IGrainInterface> : IIterativeServerGrainReader
	where IGrainInterface : IGrain
{
	#region fields
	private readonly AzureBlobGrainIteratorOptions _storageOptions;
	private readonly GrainDescriptor[] _grainDescriptions;

	private BlobContainerClient? _containerClient;
	#endregion

	#region ctor
	public AzureBlobIterativeGrainReader(
		IOptions<AzureBlobGrainIteratorOptions> storageOptions,
		params GrainDescriptor[] grainDescriptions)
	{
		_storageOptions = storageOptions.Value;
		_grainDescriptions = grainDescriptions;
	}
	#endregion

	#region IIterativeGrainReader
	public bool ReadAllowed => this is { _containerClient: not null };
	public Task<bool> StartRead(CancellationToken cancellationToken)
	{
		_containerClient = new BlobContainerClient(
			_storageOptions.ConnectionString,
			_storageOptions.ContainerName);

		return Task.FromResult(ReadAllowed);
	}
	public Task StopRead(CancellationToken cancellationToken)
	{
		_containerClient = null;
		return Task.CompletedTask;
	}
	#endregion

	#region IEnumerable<GrainId>
	public IEnumerator<GrainId?> GetEnumerator()
	{
		if (!ReadAllowed)
		{
			yield break;
		}
		foreach (var store in _grainDescriptions)
		{
			var prefix = $"{store.StateName}-{store.GrainType}";
			var regex = new Regex($"{prefix}/(?<grainId>.+).json");
			foreach (var item in _containerClient
				!.GetBlobs(prefix: prefix)
				.AsPages(default)
				.SelectMany(p => p.Values))
			{
				var match = regex.Match(item.Name);
				if (!match.Success)
				{
					continue;
				}
				var grainId = GrainId.Create(store.GrainType, match.Groups["grainId"].Value);
				yield return grainId;
			}
		}
	}
	#endregion

	#region IEnumerable
	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
	#endregion
}
