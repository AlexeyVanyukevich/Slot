namespace Slot.Domain.Entities;

public class CreditPack : Entity
{
    public int TotalCredits { get; set; }
    public int UsedCredits { get; set; }
    public DateTimeOffset ValidFrom { get; set; }
    public DateTimeOffset ValidUntil { get; set; }
    public bool IsFrozen { get; set; }
    public DateTimeOffset? FrozenAt { get; set; }
    public int TenantId { get; set; }
    public Tenant Tenant { get; set; }
    public int CustomerId { get; set; }
    public Customer Customer { get; set; }
}
