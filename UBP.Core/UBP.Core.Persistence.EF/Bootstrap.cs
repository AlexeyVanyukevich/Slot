using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using UBP.Core.Persistence.Database.Options;

namespace UBP.Core.Persistence.EF;

public static class Bootstrap
{
    public static IServiceCollection AddEFDbContext<TEFDbContext, TDbOptions>(this IServiceCollection services, Action<IServiceProvider, DbContextOptionsBuilder, TDbOptions>? dbContextBuilder = null, Action<TDbOptions>? configure = null)
        where TEFDbContext : DbContext
        where TDbOptions : DbOptions
    {
        if (configure is not null)
            services.Configure(configure);

        services.AddDbContext<TEFDbContext>((sp, builder) =>
          {
              TDbOptions options = sp.GetRequiredService<IOptions<TDbOptions>>().Value;

              dbContextBuilder?.Invoke(sp, builder, options);

          });
        return services;
    }
}