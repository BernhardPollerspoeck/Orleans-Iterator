namespace Orleans.Iterator.Azure.Blob;
public class AzureBlobGrainIteratorOptions
{
	public required string GrainStorageConnectionString { get; set; }
	public required string GrainStorageContainerName { get; set; }
}
