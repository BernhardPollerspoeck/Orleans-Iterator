using Orleans.Iterator.Dev.Grains;
using Orleans.Runtime;

namespace Orleans.Iterator.Dev.Server.Grains;

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