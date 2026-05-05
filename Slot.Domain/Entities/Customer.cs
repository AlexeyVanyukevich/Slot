using Slot.Domain.Enums;
using Slot.Domain.Interfaces;

namespace Slot.Domain.Entities;

public class Customer : Entity, IAuditable, ISoftDeletable
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public CustomerStatus Status { get; set; }
    public int TenantId { get; set; }
    public Tenant Tenant { get; set; }
    public ICollection<Booking> Bookings { get; }
    public ICollection<CreditPack> CreditPacks { get; }
    public bool IsDeleted { get; set; }
}
