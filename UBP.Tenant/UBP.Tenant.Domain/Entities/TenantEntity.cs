using UBP.Core.Entities;
using UBP.Core.Interfaces;
using UBP.Tenant.Domain.Enums;

namespace UBP.Tenant.Domain.Entities;

public class TenantEntity : Entity<Guid>, IAuditable
{
    public required string Name { get; set; }
    public required string Slug { get; set; }
    public string Timezone { get; set; }
    public TenantStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
