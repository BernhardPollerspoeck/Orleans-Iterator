namespace Orleans.Iterator.Dev.Grains;

public interface IReverseGrain : IGrainWithStringKey
{
	Task<string> Reverse();

}
