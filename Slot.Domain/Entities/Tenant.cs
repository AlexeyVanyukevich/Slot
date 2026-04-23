
using Slot.Domain.Interfaces;

namespace Slot.Domain.Entities;

public class Tenant : Entity, IAuditable, IActivatable
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string? Slug { get; set; }
    public string ConfigReference { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public bool IsActive { get; set; }
}
