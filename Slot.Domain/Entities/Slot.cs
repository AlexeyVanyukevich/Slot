using System.Diagnostics.CodeAnalysis;

using Slot.Domain.Enums;

namespace Slot.Domain.Entities;

[SuppressMessage("Naming", "CA1724", Justification = "Slot is intentional domain language matching the project name.")]
public class Slot : Entity
{
    public DateTimeOffset StartsAt { get; set; }
    public SlotStatus Status { get; set; }
    public string? CancelReason { get; set; }
    public int TenantId { get; set; }
    public Tenant Tenant { get; set; }
    public int ServiceTypeId { get; set; }
    public ServiceType ServiceType { get; set; }
    public int? ResourceId { get; set; }
    public Resource? Resource { get; set; }
    public ICollection<Booking> Bookings { get; }
}
