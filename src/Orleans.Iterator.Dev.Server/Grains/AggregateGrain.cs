using Orleans.Iterator.Abstraction;
using Orleans.Iterator.Abstraction.Server;
using Orleans.Iterator.Dev.Grains;
using System.Text;

namespace Orleans.Iterator.Dev.Server.Grains;

public class AggregateGrain : Grain, IAggregateGrain
{
	#region fields
	private readonly IServerGrainIterator _grainIterator;
	#endregion

	#region ctor
	public AggregateGrain(IServerGrainIterator grainIterator)
	{
		_grainIterator = grainIterator;
	}
	#endregion

	#region IAggregateGrain
	public async Task<string> DoWork()
	{
		var result = new StringBuilder();
		var reader = await _grainIterator.GetReader<IReverseGrain>(new GrainDescriptor("Reverse", "STORE_NAME"));
		try
		{
			await reader.StartRead(CancellationToken.None);

			foreach (var item in reader)
			{
				result.AppendLine($"[{item}]");
			}
		}
		finally
		{
			await reader.StopRead(CancellationToken.None);
		}
		return result.ToString();
	}
	#endregion
}