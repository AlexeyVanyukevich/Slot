using Microsoft.EntityFrameworkCore;

using UBP.Booking.Domain.Entities;
using UBP.Booking.Persistence.Interfaces;
using UBP.Core.Persistence.EF.Repositories;
using UBP.Core.Persistence.Interfaces;

namespace UBP.Booking.Persistence.Repositories;

internal sealed class AvailabilitySlotRepository(IDbContext dbContext) : EFRepository<AvailabilitySlotEntity>(dbContext), IAvailabilitySlotRepository
{
    public async Task<bool> TryReserveAsync(Guid slotId, CancellationToken cancellationToken = default)
    {
        var affected = await GetExpressionQuery(s => s.Id == slotId && s.BookedCount < s.Capacity)
            .ExecuteUpdateAsync(setters => setters.SetProperty(s => s.BookedCount, s => s.BookedCount + 1), cancellationToken);

        return affected == 1;
    }

    public async Task ReleaseAsync(Guid slotId, CancellationToken cancellationToken = default)
    {
        await GetExpressionQuery(s => s.Id == slotId && s.BookedCount > 0)
            .ExecuteUpdateAsync(setters => setters.SetProperty(s => s.BookedCount, s => s.BookedCount - 1), cancellationToken);
    }
}
