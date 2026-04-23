using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Slot.Persistence.Contexts;
using Slot.Persistence.Factories;
using Slot.Persistence.Interceptors;
using Slot.Persistence.Interfaces;
using Slot.Persistence.Repositories;

namespace Slot.Persistence;

public static class Bootstrap
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, Func<IServiceProvider, Options.DbContextOptions> dbContextOptionsFactory)
    {
        services.AddDbContexts(dbContextOptionsFactory);
        services.AddServices();
        return services;
    }

    private static IServiceCollection AddDbContexts(this IServiceCollection services, Func<IServiceProvider, Options.DbContextOptions> dbContextOptionsFactory)
    {
        services.AddDbContextPool<AppDbContext>((sp, builder) => {
            var options = dbContextOptionsFactory(sp);
            builder.UseNpgsql(options.ConnectionString).AddInterceptors(
                sp.GetRequiredService<AuditableInterceptor>());
        });

        return services;
    }

    private static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped(typeof(EntityRepository<>), typeof(EntityRepository<>));
        services.AddScoped<IEntityRepositoryFactory, EntityRepositoryFactory>();

        return services;
    }
}
