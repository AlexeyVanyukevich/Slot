using Slot.Domain.Enums;
using Slot.Domain.Interfaces;

namespace Slot.Domain.Entities;

public class Booking : Entity, IAuditable
{
    public BookingStatus Status { get; set; }
    public int TenantId { get; set; }
    public Tenant Tenant { get; set; }
    public int CustomerId { get; set; }
    public Customer Customer { get; set; }
    public int SlotId { get; set; }
    public SlotEntity Slot { get; set; }
    public int CreditPackId { get; set; }
    public CreditPack CreditPack { get; set; }
    public int CreditsConsumed { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? CancelledAt { get; set; }
    public string? CancelReason { get; set; }
}
