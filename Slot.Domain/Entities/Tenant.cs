
using Slot.Domain.Interfaces;

namespace Slot.Domain.Entities;

public class Tenant : Entity, IAuditable, IActivatable, ISoftDeletable
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string? Slug { get; set; }
    public string ConfigReference { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public bool IsActive { get; set; }
    public ICollection<Customer> Customers { get; }
    public ICollection<Resource> Resources { get; }
    public ICollection<ServiceType> ServiceTypes { get; }
    public ICollection<Slot> Slots { get; }
    public ICollection<Booking> Bookings { get; }
    public ICollection<CreditPack> CreditPacks { get; }
    public bool IsDeleted { get; set; }

}
