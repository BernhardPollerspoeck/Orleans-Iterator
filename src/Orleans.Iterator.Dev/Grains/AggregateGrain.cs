using Orleans.Iterator.Abstraction;
using System.Text;

namespace Orleans.Iterator.Dev.Grains;

public class AggregateGrain : Grain, IAggregateGrain
{
    #region fields
    private readonly IGrainIterator _grainIterator;
    #endregion

    #region ctor
    public AggregateGrain(IGrainIterator grainIterator)
    {
        _grainIterator = grainIterator;
    }
    #endregion

    #region IAggregateGrain
    public async Task<string> DoWork()
    {
        var result = new StringBuilder();
        var reader = await _grainIterator.GetReader<IReverseGrain>("Reverserino");
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