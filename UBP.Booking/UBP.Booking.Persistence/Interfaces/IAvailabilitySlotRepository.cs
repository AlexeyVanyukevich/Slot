using UBP.Booking.Domain.Entities;
using UBP.Core.Persistence.Interfaces;

namespace UBP.Booking.Persistence.Interfaces;

public interface IAvailabilitySlotRepository : IRepository<AvailabilitySlotEntity>
{
    Task<bool> TryReserveAsync(Guid slotId, CancellationToken cancellationToken = default);
    Task ReleaseAsync(Guid slotId, CancellationToken cancellationToken = default);
}
