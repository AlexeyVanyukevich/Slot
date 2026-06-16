using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

using UBP.Core.Data.EF.Factories;
using UBP.Core.Data.EF.Interceptors;
using UBP.Core.Persistence;
using UBP.Core.Persistence.Interfaces;

namespace UBP.Core.Data.EF;

public static class Bootstrap
{
    public static IServiceCollection AddInterceptors(this IServiceCollection services)
    {
        services.AddSingleton<AuditableInterceptor>();
        services.AddSingleton<SoftDeletableInterceptor>();
        return services;
    }


    public static IEnumerable<IInterceptor> GetInterceptors(IServiceProvider sp)
    {
        yield return sp.GetRequiredService<AuditableInterceptor>();
        yield return sp.GetRequiredService<SoftDeletableInterceptor>();
    }

    public static IServiceCollection AddUnitOfWork(this IServiceCollection services, Func<IServiceProvider, IDbContext> dbContextFactory)
    {
        services.AddScoped<RepositoryFactory>();
        services.AddUnitOfWork(dbContextFactory, sp => sp.GetRequiredService<RepositoryFactory>());
        return services;
    }

}