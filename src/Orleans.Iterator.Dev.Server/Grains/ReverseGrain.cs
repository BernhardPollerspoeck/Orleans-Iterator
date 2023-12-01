using Orleans.Iterator.Dev.Grains;
using Orleans.Runtime;

namespace Orleans.Iterator.Dev.Server.Grains;

[GrainType("Reverse2")]
public class ReverseGrain : Grain, IReverseGrain
{
    #region fields
    private readonly IPersistentState<ReverseState> _state;
    #endregion

    #region ctor
    public ReverseGrain(
        [PersistentState("Reverse","STORE_NAME")]
        IPersistentState<ReverseState> state)
    {
        _state = state;
    }
    #endregion

    #region IReverseGrain
    public async Task<string> Reverse()
    {
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
    #endregion
}

[GrainType("Reversed2")]
public class AnotherReverseGrain : Grain, IReverseGrain
{
    #region fields
    private readonly IPersistentState<ReverseState> _state;
    #endregion


    #region ctor
    public AnotherReverseGrain(
        [PersistentState("Reversed","STORE_NAME")]
        IPersistentState<ReverseState> state)
    {
        _state = state;
    }
    #endregion


    public async Task<string> Reverse()
    {
        _state.State = new()
        {
            Reverse = this.GetPrimaryKeyString(),
        };
        await _state.WriteStateAsync();
        return this.GetPrimaryKeyString();
    }
}