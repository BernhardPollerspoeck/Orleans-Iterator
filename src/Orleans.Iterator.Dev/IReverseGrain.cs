namespace Orleans.Iterator.Dev;

public interface IReverseGrain : IGrainWithStringKey
{
    Task<string> Reverse();

}
