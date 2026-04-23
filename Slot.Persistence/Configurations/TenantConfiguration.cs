using Slot.Domain.Entities;

namespace Slot.Persistence.Configurations;

internal sealed class TenantConfiguration() : EntityConfiguration<Tenant>(TableName)
{
    public const string TableName = "tenants";
}
