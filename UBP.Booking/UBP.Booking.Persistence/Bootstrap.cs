using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using UBP.Booking.Persistence.Contexts;
using UBP.Booking.Persistence.Interfaces;
using UBP.Booking.Persistence.Repositories;
using UBP.Core.Data.EF;

namespace UBP.Booking.Persistence;

public static class Bootstrap
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, Action<IServiceProvider, DbContextOptionsBuilder> optionsAction)
    {
        services.AddInterceptors();

        services.AddDbContextPool<AppDbContext>(optionsAction);
        services.AddUnitOfWork(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IAvailabilitySlotRepository, AvailabilitySlotRepository>();

        return services;
    }
}
