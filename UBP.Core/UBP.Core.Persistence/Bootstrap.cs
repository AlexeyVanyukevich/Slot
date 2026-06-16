using Microsoft.Extensions.DependencyInjection;

using UBP.Core.Persistence.Factories;
using UBP.Core.Persistence.Interfaces;
namespace UBP.Core.Persistence;

public static class Bootstrap
{
    public static IServiceCollection AddUnitOfWork(
        this IServiceCollection services, 
        Func<IServiceProvider, IDbContext> dbContextFactory,
        Func<IServiceProvider, IRepositoryFactory>? repositoryFactoryFactory = null)
    {
        if (repositoryFactoryFactory != null)
        {
            services.AddScoped<IRepositoryFactory>(repositoryFactoryFactory);
        } else
        {
            services.AddScoped<IRepositoryFactory, RepositoryFactory>();
        }
        services.AddScoped<IDbContext>(dbContextFactory);
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        return services;
    }

}