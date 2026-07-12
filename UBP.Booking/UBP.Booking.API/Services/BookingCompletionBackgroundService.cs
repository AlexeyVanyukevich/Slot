using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using UBP.Booking.Application.Commands;
using UBP.CQRS;

namespace UBP.Booking.API.Services;

internal sealed class BookingCompletionBackgroundService(IServiceScopeFactory scopeFactory) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Interval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            using var scope = scopeFactory.CreateScope();
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            await sender.SendAsync(new CompleteExpiredBookingsCommand(), stoppingToken);
        }
    }
}
