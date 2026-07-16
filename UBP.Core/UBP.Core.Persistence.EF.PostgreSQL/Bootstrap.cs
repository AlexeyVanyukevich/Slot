using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using UBP.Core.Persistence.Database.Options;

namespace UBP.Core.Persistence.EF.PostgreSQL;

public static class Bootstrap
{
    public static IServiceCollection AddPostgresEFDbContext<TEFDbContext, TDbOptions>(this IServiceCollection services, Action<TDbOptions>? configure = null)
        where TEFDbContext : DbContext
        where TDbOptions : DbOptions
    {
        services.AddEFDbContext<TEFDbContext, TDbOptions>((sp, builder, _) =>
          {
              TDbOptions options = sp.GetRequiredService<IOptions<TDbOptions>>().Value;

              builder.UseNpgsql(options.ConnectionString)
                     .UseSnakeCaseNamingConvention();
          }, configure);
        return services;
    }
}