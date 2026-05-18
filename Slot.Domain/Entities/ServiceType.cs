using Slot.Domain.Interfaces;

namespace Slot.Domain.Entities;

public class ServiceType : Entity, IAuditable
{
    public string Name { get; set; }
    public int DurationMinutes { get; set; }
    public int Capacity { get; set; }
    public bool RequiresResource { get; set; }
    public int CreditCost { get; set; }
    public string? Description { get; set; }
    public int TenantId { get; set; }
    public Tenant Tenant { get; set; }
    public ICollection<SlotEntity> Slots { get; }
    public DateTimeOffset CreatedAt { get; set; }
}
