using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;


namespace UBP.Core.Persistence.EF;

public static class Bootstrap
{
    public static IServiceCollection AddDbContext<TDbContext>(
    this IServiceCollection services,
    Action<IServiceProvider, DbContextOptionsBuilder> dbContextOptionsBuilder) where TDbContext : DbContext
    {
        services.AddDbContextPool<TDbContext>((sp, builder) =>
        {
            dbContextOptionsBuilder(sp, builder);
        });
        return services;
    }
}
