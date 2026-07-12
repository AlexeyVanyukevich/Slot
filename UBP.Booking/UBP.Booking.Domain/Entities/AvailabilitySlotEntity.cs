using UBP.Core.Entities;

namespace UBP.Booking.Domain.Entities;

public class AvailabilitySlotEntity : Entity<Guid>
{
    public required Guid TenantId { get; set; }
    public required Guid ResourceId { get; set; }
    public required DateTime StartAt { get; set; }
    public required DateTime EndAt { get; set; }
    public int Capacity { get; set; } = 1;
    public int BookedCount { get; set; }
}
