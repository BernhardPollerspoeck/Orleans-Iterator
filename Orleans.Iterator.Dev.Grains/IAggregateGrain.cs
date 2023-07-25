namespace Orleans.Iterator.Dev.Grains;
public interface IAggregateGrain : IGrainWithStringKey
{
    Task<string> DoWork();
}
