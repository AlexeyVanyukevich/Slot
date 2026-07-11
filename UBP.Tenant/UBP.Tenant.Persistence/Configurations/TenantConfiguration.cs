
using UBP.Core.Data.EF.Configurations;
using UBP.Tenant.Domain.Entities;

namespace UBP.Tenant.Persistence.Configurations;

internal sealed class TenantConfiguration() : EntityConfiguration<TenantEntity, Guid>(TableName)
{
    public const string TableName = "tenants";
}
