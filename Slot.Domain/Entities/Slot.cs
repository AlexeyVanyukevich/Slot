using System.Diagnostics.CodeAnalysis;

using Slot.Domain.Enums;

namespace Slot.Domain.Entities;

public class SlotEntity : Entity
{
    public DateTimeOffset StartsAt { get; set; }
    public SlotStatus Status { get; set; }
    public string? CancelReason { get; set; }
    public int TenantId { get; set; }
    public Tenant Tenant { get; set; }
    public int ServiceTypeId { get; set; }
    public ServiceType ServiceType { get; set; }
    public ICollection<SlotResource> Resources { get; }
    public ICollection<Booking> Bookings { get; }
}
