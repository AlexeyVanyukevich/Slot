using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace UBP.Options.Configuration;

public class BindConfigureOptions<TOptions>(IConfiguration configuration) : IConfigureOptions<TOptions> where TOptions : class
{
    protected IConfiguration Configuration => configuration;
    public virtual void Configure(TOptions options)
    {
        Configuration.Bind(options);
    }
}
