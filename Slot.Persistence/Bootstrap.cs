using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Slot.Persistence.Abstractions.Interfaces;
using Slot.Persistence.Contexts;
using Slot.Persistence.Factories;
using Slot.Persistence.Interceptors;
namespace Slot.Persistence;

public static class Bootstrap
{
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        Func<IServiceProvider, Options.DbContextOptions> dbContextOptionsFactory)
    {
        services.AddInterceptors();
        services.AddDbContexts(dbContextOptionsFactory);
        services.AddUnitOfWork();
        return services;
    }

    private static IServiceCollection AddInterceptors(this IServiceCollection services)
    {
        services.AddSingleton<AuditableInterceptor>();
        services.AddSingleton<SoftDeletableInterceptor>();
        return services;
    }

    private static IServiceCollection AddDbContexts(
        this IServiceCollection services,
        Func<IServiceProvider, Options.DbContextOptions> dbContextOptionsFactory)
    {
        EFCore.Bootstrap.AddDbContext<AppDbContext>(services, (sp, builder) =>
        {
            var options = dbContextOptionsFactory(sp);
            builder.UseNpgsql(options.ConnectionString)
                   .AddInterceptors(
                       sp.GetRequiredService<AuditableInterceptor>(),
                       sp.GetRequiredService<SoftDeletableInterceptor>());
        });
        return services;
    }


    private static IServiceCollection AddUnitOfWork(this IServiceCollection services)
    {
        services.AddScoped<IRepositoryFactory, RepositoryFactory>();
        services.AddScoped<Interfaces.IUnitOfWork, UnitOfWork>();
        services.AddScoped<Abstractions.Interfaces.IUnitOfWork>(sp => sp.GetRequiredService<Interfaces.IUnitOfWork>());
        return services;
    }

}