using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using UBP.Core.Data.EF;
using UBP.Tenant.Persistence.Contexts;

namespace UBP.Tenant.Persistence;

public static class Bootstrap
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, Action<IServiceProvider, DbContextOptionsBuilder> optionsAction)
    {
        services.AddInterceptors();

        services.AddDbContextPool<AppDbContext>(optionsAction);
        services.AddUnitOfWork(sp => sp.GetRequiredService<AppDbContext>());

        return services;
    }
}
