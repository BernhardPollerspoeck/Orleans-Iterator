using System.Data;

namespace Orleans.Iterator.Dev.COPY;

internal interface ICommandInterceptor
{
    void Intercept(IDbCommand command);
}
