namespace Orleans.Iterator.Azure.Blob;
public class AzureBlobGrainIteratorOptions
{
	public required string ConnectionString { get; set; }
	public required string ContainerName { get; set; }
}
