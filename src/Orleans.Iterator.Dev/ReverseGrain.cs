using Orleans.Runtime;
using System.Buffers.Text;
using System.Text;

namespace Orleans.Iterator.Dev;

public class ReverseGrain : Grain, IReverseGrain
{
    private readonly IPersistentState<ReverseState> _state;

    public ReverseGrain(
        [PersistentState("Reverserino","STORE_NAME")]
        IPersistentState<ReverseState> state)
    {
        _state = state;
    }


    public async Task<string> Reverse()
    {
        var t = TryGetIntegerKey(this.GetGrainId(), out var key, out var ext);
        
        
        if (_state is { State.Reverse.Length: > 0 })
        {
            return _state.State.Reverse;
        }
        var reversed = new string(this.GetPrimaryKeyString().Reverse().ToArray());
        _state.State = new()
        {
            Reverse = reversed,
        };
        await _state.WriteStateAsync();


        return reversed;
    }


    public static bool TryGetIntegerKey(GrainId grainId, out long key, out string? keyExt)
    {
        keyExt = null;
        var keyString = grainId.Key.AsSpan();
        if (keyString.IndexOf((byte)'+') is int index && index >= 0)
        {
            keyExt = Encoding.UTF8.GetString(keyString.Slice(index + 1));
            keyString = keyString.Slice(0, index);
        }

        return Utf8Parser.TryParse(keyString, out key, out var len, 'X') && len == keyString.Length;
    }
}

public class ReverseState
{
    public string? Reverse { get; set; }
}
