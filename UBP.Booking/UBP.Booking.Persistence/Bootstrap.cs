using Microsoft.Extensions.DependencyInjection;

using UBP.Booking.Persistence.Contexts;
using UBP.Booking.Persistence.Interfaces;
using UBP.Booking.Persistence.Repositories;
using UBP.Core.Data.EF;
using UBP.Core.Persistence.Database.Options;
using UBP.Core.Persistence.EF.PostgreSQL;

namespace UBP.Booking.Persistence;

public static class Bootstrap
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, Action<DbOptions>? configure = null)
    {
        services.AddInterceptors();

        services.AddPostgresEFDbContext<AppDbContext, DbOptions>(configure);
        services.AddUnitOfWork(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IAvailabilitySlotRepository, AvailabilitySlotRepository>();

        return services;
    }
}
