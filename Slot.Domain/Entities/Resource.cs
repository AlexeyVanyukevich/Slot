using Slot.Domain.Enums;
using Slot.Domain.Interfaces;

namespace Slot.Domain.Entities;

public class Resource : Entity, IAuditable, IActivatable
{
    public string Name { get; set; }
    public string ConfigReference { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public bool IsActive { get; set; }
    public int TenantId { get; set; }
    public Tenant Tenant { get; set; }
    public ICollection<SlotResource> Slots { get; }
}
