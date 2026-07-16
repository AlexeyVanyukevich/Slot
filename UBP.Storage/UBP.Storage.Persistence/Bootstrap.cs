using Microsoft.Extensions.DependencyInjection;

using UBP.Core.Data.EF;
using UBP.Core.Persistence.Database.Options;
using UBP.Core.Persistence.EF.PostgreSQL;
using UBP.Storage.Persistence.Contexts;

namespace UBP.Storage.Persistence;

public static class Bootstrap
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, Action<DbOptions>? configure = null)
    {
        services.AddPostgresEFDbContext<AppDbContext, DbOptions>(configure);
        services.AddUnitOfWork(sp => sp.GetRequiredService<AppDbContext>());

        return services;
    }
}
