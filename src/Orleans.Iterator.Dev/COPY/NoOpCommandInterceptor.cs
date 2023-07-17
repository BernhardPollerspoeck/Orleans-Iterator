using System.Data;

namespace Orleans.Iterator.Dev.COPY;

internal class NoOpCommandInterceptor : ICommandInterceptor
{
    public static readonly ICommandInterceptor Instance = new NoOpCommandInterceptor();

    private NoOpCommandInterceptor()
    {

    }

    public void Intercept(IDbCommand command)
    {
        //NOP
    }
}
